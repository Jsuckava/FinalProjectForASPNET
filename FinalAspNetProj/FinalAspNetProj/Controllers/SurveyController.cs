using AutoMapper;
using FinalAspNetProj.Data;
using FinalAspNetProj.Documents;
using FinalAspNetProj.DTO;
using FinalAspNetProj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

[Route("api/survey")]
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

    [HttpGet("comments")]
    public async Task<ActionResult<IEnumerable<object>>> GetRecentComments()
    {
        try
        {
            var comments = await _context.Set<Survey>()
                .AsNoTracking()
                .Where(s => s.Comment != null && s.Comment != "")
                .OrderByDescending(s => s.DateCompleted)
                .Take(10)
                .Select(s => new
                {
                    Name = s.RespondentName,
                    Date = s.DateCompleted,
                    Text = s.Comment
                })
                .ToListAsync();

            return Ok(comments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error loading comments.", details = ex.Message });
        }
    }

    [HttpGet("active-template")]
    public async Task<ActionResult<SurveyTemplate_ReadDTO>> GetActiveSurveyTemplate()
    {
        try
        {
            var template = await _context.Set<SurveyTemplate>()
                .Include(t => t.Questions)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IsActive);

            if (template == null)
            {
                return NotFound(new { message = "No active survey template found." });
            }
            var templateDto = _mapper.Map<SurveyTemplate_ReadDTO>(template);
            templateDto.QuestionCount = template.Questions.Count;

            return Ok(templateDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving active survey template details.", details = ex.Message });
        }
    }

    [HttpGet("questions")]
    public async Task<ActionResult<IEnumerable<SurveyQuestion_ReadDTO>>> GetSurveyQuestions()
    {
        try
        {
            var questions = await _context.Set<SurveyTemplate>()
                .AsNoTracking()
                .Where(t => t.IsActive)
                .SelectMany(t => t.Questions)
                .Select(q => new SurveyQuestion_ReadDTO
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.Text,
                    MaxRating = q.MaxRating
                })
                .ToListAsync();

            if (questions == null || questions.Count == 0)
            {
                return NotFound(new { message = "No active survey template or no questions found." });
            }

            return Ok(questions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while loading active survey questions.", details = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> PostSurvey(Survey_CreateDTO dto)
    {
        try
        {
            var questionIds = dto.Responses.Select(r => r.QuestionId).Distinct().ToList();
            var questionData = await _context.Set<Question>()
                .Where(q => questionIds.Contains(q.QuestionId))
                .Select(q => new { q.QuestionId, q.MaxRating })
                .ToDictionaryAsync(q => q.QuestionId, q => q);

            if (questionData.Count != questionIds.Count)
            {
                var missingIds = questionIds.Where(id => !questionData.ContainsKey(id)).ToList();
                return BadRequest(new
                {
                    message = "One or more Question IDs are invalid.",
                    missingIds = missingIds
                });
            }

            var respondentName = string.IsNullOrWhiteSpace(dto.RespondentName) ? "Anonymous" : dto.RespondentName;

            var newSurvey = new Survey
            {
                RespondentName = respondentName,
                DateCompleted = DateTime.UtcNow,
                Comment = dto.Comment 
            };
            _context.Set<Survey>().Add(newSurvey);
            await _context.SaveChangesAsync();

            int totalMaxScore = 0;
            int totalScore = 0;

            foreach (var responseDto in dto.Responses)
            {
                if (!questionData.ContainsKey(responseDto.QuestionId))
                {
                    return BadRequest(new { message = $"Question ID {responseDto.QuestionId} not found in loaded data." });
                }

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
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while saving the survey.",
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });
        }
    }

    [HttpGet("analysis/{id}")]
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

    [HttpGet("download")]
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