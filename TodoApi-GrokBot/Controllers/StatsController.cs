using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApiGrokBot.Extensions;
using TodoApiGrokBot.Models.Dtos;
using TodoApiGrokBot.Services;

namespace TodoApiGrokBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController(IStatsService stats) : ControllerBase
{
    [Authorize]
    [HttpGet("completions")]
    public Task<List<DailyCount>> Completions() => stats.GetCompletionsAsync(User.GetUserId());

    [AllowAnonymous]
    [HttpGet("monthly-active-users")]
    public Task<List<MonthlyActiveUsers>> MonthlyActiveUsers() => stats.GetMonthlyActiveUsersAsync();
}
