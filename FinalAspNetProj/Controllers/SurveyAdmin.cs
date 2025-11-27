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


    [HttpPost]
    public async Task<IActionResult> CreateSurveyTemplate(SurveyTemplate_CreateDTO dto)
    {
        var newTemplate = _mapper.Map<SurveyTemplate>(dto);

        newTemplate.Questions = dto.Questions.Select(qDto => new Question
        {
            Text = qDto.Text,
            MaxRating = qDto.MaxRating
        }).ToList();

        _context.Set<SurveyTemplate>().Add(newTemplate);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSurveyTemplate(int id, SurveyTemplate_UpdateDTO dto)
    {
        var template = await _context.Set<SurveyTemplate>()
            .Include(t => t.Questions)
            .FirstOrDefaultAsync(t => t.SurveyTemplateID == id);

        if (template == null)
        {
            return NotFound(new { message = $"Survey Template ID {id} not found." });
        }

        _mapper.Map(dto, template);

        _context.Set<Question>().RemoveRange(template.Questions);
        template.Questions = dto.Questions.Select(qDto => new Question
        {
            Text = qDto.Text,
            MaxRating = qDto.MaxRating,
            SurveyTemplateID = template.SurveyTemplateID
        }).ToList();

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!_context.Set<SurveyTemplate>().Any(e => e.SurveyTemplateID == id))
        {
            return NotFound(new { message = $"Survey Template ID {id} not found." });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSurveyTemplate(int id)
    {
        var template = await _context.Set<SurveyTemplate>().FindAsync(id);
        if (template == null)
        {
            return NotFound(new { message = $"Survey Template ID {id} not found." });
        }

        _context.Set<SurveyTemplate>().Remove(template);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}