using AutoMapper;
using FinalAspNetProj.Data;
using FinalAspNetProj.Documents;
using FinalAspNetProj.DTO;
using FinalAspNetProj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

[Route("api/Survey")]
[ApiController]
public class SurveyController : ControllerBase
{
    private readonly AspnetfpDbContext _context;
    private readonly IMapper _mapper;

    public SurveyController(AspnetfpDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/Survey/questions
    [HttpGet("questions")]
    public async Task<ActionResult<IEnumerable<SurveyQuestion_ReadDTO>>> GetSurveyQuestions()
    {
        var questions = await _context.SurveyQuestions
            .AsNoTracking()
            .Select(q => new SurveyQuestion_ReadDTO
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                MaxRating = q.MaxRating
            })
            .ToListAsync();

        if (questions.Count == 0)
        {
            return NotFound(new { message = "No survey questions found." });
        }

        return Ok(questions);
    }

    // POST: api/Survey
    [HttpPost]
    public async Task<ActionResult> PostSurvey(Survey_CreateDTO dto)
    {
        var questionIds = dto.Responses.Select(r => r.QuestionId).Distinct().ToList();

        // Validate against SurveyQuestions, not Question
        var questionData = await _context.SurveyQuestions
            .Where(q => questionIds.Contains(q.QuestionId))
            .Select(q => new { q.QuestionId, q.MaxRating })
            .ToDictionaryAsync(q => q.QuestionId, q => q);

        if (questionData.Count != questionIds.Count)
        {
            return BadRequest(new { message = "One or more Question IDs are invalid." });
        }

        var newSurvey = new Survey
        {
            RespondentName = dto.RespondentName,
            DateCompleted = DateTime.UtcNow
        };

        _context.Surveys.Add(newSurvey);
        await _context.SaveChangesAsync();

        int totalMaxScore = 0;
        int totalScore = 0;

        foreach (var responseDto in dto.Responses)
        {
            var question = questionData[responseDto.QuestionId];

            if (responseDto.Rating < 1 || responseDto.Rating > question.MaxRating)
            {
                return BadRequest(new { message = $"Rating for Question ID {question.QuestionId} must be between 1 and {question.MaxRating}." });
            }

            _context.SurveyResponses.Add(new SurveyResponse
            {
                SurveyId = newSurvey.SurveyId,
                QuestionId = responseDto.QuestionId,
                Rating = responseDto.Rating
            });

            totalMaxScore += question.MaxRating;
            totalScore += responseDto.Rating;
        }

        decimal percentageScore = totalMaxScore > 0 ? (decimal)totalScore / totalMaxScore * 100 : 0;
        decimal flawProbability = 100 - percentageScore;
        decimal reUseProbability = percentageScore > 75 ? 95m : (percentageScore * 1.2m);

        var analysis = new SurveyAnalysis
        {
            SurveyId = newSurvey.SurveyId,
            TotalScore = totalScore,
            PercentageScore = Math.Round(percentageScore, 2),
            FlawProbability = Math.Round(flawProbability, 2),
            ReUseProbability = Math.Round(Math.Min(100, reUseProbability), 2)
        };

        _context.SurveyAnalysis.Add(analysis);
        await _context.SaveChangesAsync();

        return Ok();
    }

    // GET: api/Survey/analysis/{id}
    [HttpGet("analysis/{id}")]
    [Authorize]
    public async Task<ActionResult<SurveyAnalysis_ReadDTO>> GetSurveyAnalysis(int id)
    {
        var analysis = await _context.SurveyAnalysis
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.SurveyId == id);

        if (analysis == null)
        {
            return NotFound(new { message = $"Analysis for Survey ID {id} not found." });
        }

        return Ok(_mapper.Map<SurveyAnalysis_ReadDTO>(analysis));
    }

    // GET: api/Survey/download?format=pdf
    [HttpGet("download")]
    [Authorize]
    public async Task<IActionResult> DownloadSurveys([FromQuery] string format = "pdf")
    {
        if (format.ToLower() != "pdf")
        {
            return BadRequest("Invalid format specified. Only 'pdf' is supported.");
        }

        var allSurveys = await _context.Surveys
            .Include(s => s.SurveyAnalysis)
            .AsNoTracking()
            .OrderByDescending(s => s.DateCompleted)
            .ToListAsync();

        var document = new SurveyReportDocument(allSurveys);
        var fileBytes = document.GeneratePdf();
        var fileName = $"SurveyResponses_{DateTime.Now:yyyyMMddHHmmss}.pdf";

        var newFileRecord = new DownloadableFile
        {
            FileName = fileName,
            DateCreated = DateOnly.FromDateTime(DateTime.Now),
            FileType = "PDF",
            FilePath = $"/exports/pdf/{DateTime.Now:yyyyMMddHHmmss}"
        };

        _context.DownloadableFiles.Add(newFileRecord);
        await _context.SaveChangesAsync();

        return File(fileBytes, "application/pdf", fileName);
    }
}
