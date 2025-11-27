using System;
using Microsoft.AspNetCore.Authorization;
using FinalAspNetProj.Data;
using FinalAspNetProj.DTO;
using FinalAspNetProj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/Survey/Stats")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AspnetfpDbContext _context;

    public DashboardController(AspnetfpDbContext context)
    {
        _context = context;
    }

    [HttpGet("Total")]
    public async Task<ActionResult<CountDTO>> GetTotalResponses()
    {
        var count = await _context.Set<Survey>().CountAsync();
        return Ok(new CountDTO { Count = count });
    }

    [HttpGet("AverageScore")]
    public async Task<ActionResult<ScoreDTO>> GetAverageScore()
    {
        if (!await _context.Set<SurveyAnalysis>().AnyAsync())
        {
            return Ok(new ScoreDTO { Score = 0m });
        }

        var avg = await _context.Set<SurveyAnalysis>()
            .AverageAsync(a => (decimal?)a.PercentageScore);

        return Ok(new ScoreDTO { Score = avg ?? 0m });
    }

    [HttpGet("CompletionRate")]
    public ActionResult<PercentageDTO> GetCompletionRate()
    {
        return Ok(new PercentageDTO { Percentage = 100 });
    }

    [HttpGet("Today")]
    public async Task<ActionResult<CountDTO>> GetTodayResponses()
    {
        var today = DateTime.Today;
        var count = await _context.Set<Survey>()
            .CountAsync(s => s.DateCompleted.Date == today);

        return Ok(new CountDTO { Count = count });
    }

    [HttpGet("Trends")]
    public async Task<ActionResult<ChartDataDTO>> GetTrends([FromQuery] int days = 30)
    {
        if (days <= 0 || days > 90)
        {
            return BadRequest("Days must be between 1 and 90.");
        }

        var dateThreshold = DateTime.Today.AddDays(-days + 1);

        var dailyData = await _context.Set<Survey>()
            .Where(s => s.DateCompleted.Date >= dateThreshold)
            .GroupBy(s => s.DateCompleted.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var chartData = new ChartDataDTO();
        var dataPoints = new List<int>();
        var currentDate = dateThreshold;

        for (int i = 0; i < days; i++)
        {
            chartData.Labels.Add(currentDate.ToString("MMM d"));

            var dailyCount = dailyData.FirstOrDefault(d => d.Date == currentDate.Date)?.Count ?? 0;
            dataPoints.Add(dailyCount);

            currentDate = currentDate.AddDays(1);
        }

        chartData.Datasets.Add(new ChartDataset
        {
            Label = "Daily Responses",
            Data = dataPoints
        });

        return Ok(chartData);
    }
}