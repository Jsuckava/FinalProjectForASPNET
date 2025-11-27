using AutoMapper;
using FinalAspNetProj.Data;
using FinalAspNetProj.Models;
using FinalAspNetProj.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[Route("api/SurveyTemplates")]
[ApiController]
[Authorize]
public class SurveyAdminController : ControllerBase
{
    private readonly AspnetfpDbContext _context;
    private readonly IMapper _mapper;

    public SurveyAdminController(AspnetfpDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SurveyTemplate_ReadDTO>>> GetSurveyTemplates()
    {
        var templates = await _context.Set<SurveyTemplate>()
            .Select(t => new SurveyTemplate_ReadDTO
            {
                SurveyTemplateID = t.SurveyTemplateID,
                Title = t.Title,
                Description = t.Description,
                IsActive = t.IsActive,
                QuestionCount = t.Questions.Count
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(templates);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SurveyTemplate_ReadDTO>> GetSurveyTemplate(int id)
    {
        try
        {
            var template = await _context.Set<SurveyTemplate>()
                .Include(t => t.Questions)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.SurveyTemplateID == id);

            if (template == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<SurveyTemplate_ReadDTO>(template));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while retrieving the survey template.",
                details = ex.Message
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateSurveyTemplate(SurveyTemplate_CreateDTO dto)
    {
        foreach (var qDto in dto.Questions)
        {
            if (string.IsNullOrWhiteSpace(qDto.QuestionText))
            {
                return BadRequest(new { message = "All survey questions must contain valid text." });
            }
        }

        var newTemplate = _mapper.Map<SurveyTemplate>(dto);

        newTemplate.Questions = dto.Questions.Select(qDto => new Question
        {
            Text = qDto.QuestionText,
            MaxRating = qDto.MaxRating
        }).ToList();

        _context.Set<SurveyTemplate>().Add(newTemplate);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSurveyTemplate), new { id = newTemplate.SurveyTemplateID }, _mapper.Map<SurveyTemplate_ReadDTO>(newTemplate));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSurveyTemplate(int id, SurveyTemplate_UpdateDTO dto)
    {
        foreach (var qDto in dto.Questions)
        {
            if (string.IsNullOrWhiteSpace(qDto.QuestionText))
            {
                return BadRequest(new { message = "All survey questions must contain valid text." });
            }
        }
        var template = await _context.Set<SurveyTemplate>()
            .Include(t => t.Questions)
            .FirstOrDefaultAsync(t => t.SurveyTemplateID == id);

        if (template == null)
        {
            return NotFound(new { message = $"Survey Template ID {id} not found." });
        }

        _mapper.Map(dto, template);
        var incomingQuestionIds = dto.Questions
            .Where(q => q.QuestionId.HasValue)
            .Select(q => q.QuestionId.Value)
            .ToList();
        var questionsToDelete = template.Questions
            .Where(q => !incomingQuestionIds.Contains(q.QuestionId))
            .ToList();

        if (questionsToDelete.Any())
        {
            _context.Set<Question>().RemoveRange(questionsToDelete);
        }

        foreach (var qDto in dto.Questions)
        {
            if (qDto.QuestionId.HasValue && qDto.QuestionId > 0)
            {
                var existingQuestion = template.Questions
                    .FirstOrDefault(q => q.QuestionId == qDto.QuestionId.Value);

                if (existingQuestion != null)
                {
                    existingQuestion.Text = qDto.QuestionText;
                    existingQuestion.MaxRating = qDto.MaxRating;
                }
            }
            else
            {
                template.Questions.Add(new Question
                {
                    Text = qDto.QuestionText,
                    MaxRating = qDto.MaxRating,
                    SurveyTemplateID = id
                });
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Set<SurveyTemplate>().Any(e => e.SurveyTemplateID == id))
            {
                return NotFound(new { message = $"Survey Template ID {id} not found." });
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSurveyTemplate(int id)
    {
        var template = await _context.Set<SurveyTemplate>().FindAsync(id);
        if (template == null) return NotFound();
        template.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}