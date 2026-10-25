using Microsoft.EntityFrameworkCore;
using TodoApiGrokBot.Data;
using TodoApiGrokBot.Models.Dtos;

namespace TodoApiGrokBot.Services;

public class StatsService(AppDbContext db, TimeProvider clock) : IStatsService
{
    // 台北固定 UTC+8（不依賴容器內 tzdata）
    private static readonly TimeSpan Offset = TimeSpan.FromHours(8);

    private DateTime TaipeiNow => clock.GetUtcNow().UtcDateTime + Offset;

    public async Task<List<DailyCount>> GetCompletionsAsync(int userId, int days = 30)
    {
        var firstDay = TaipeiNow.Date.AddDays(-(days - 1));
        var fromUtc = DateTime.SpecifyKind(firstDay - Offset, DateTimeKind.Utc);

        var completed = await db.Todos
            .Where(t => t.UserId == userId && t.CompletedAt != null && t.CompletedAt >= fromUtc)
            .Select(t => t.CompletedAt!.Value)
            .ToListAsync();

        var byDay = completed.GroupBy(c => (c + Offset).Date).ToDictionary(g => g.Key, g => g.Count());
        return Enumerable.Range(0, days)
            .Select(i => firstDay.AddDays(i))
            .Select(d => new DailyCount(d.ToString("yyyy-MM-dd"), byDay.GetValueOrDefault(d)))
            .ToList();
    }

    public async Task<List<MonthlyActiveUsers>> GetMonthlyActiveUsersAsync(int months = 12)
    {
        var now = TaipeiNow;
        var firstMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1));
        var fromUtc = DateTime.SpecifyKind(firstMonth - Offset, DateTimeKind.Utc);

        // 只取 (UserId, 台北年月) 不重複組合，不回傳任何使用者資料
        var pairs = await db.LoginEvents
            .Where(e => e.OccurredAt >= fromUtc)
            .Select(e => new { e.UserId, e.OccurredAt })
            .ToListAsync();

        var byMonth = pairs
            .Select(p => (p.UserId, Month: (p.OccurredAt + Offset).ToString("yyyy-MM")))
            .Distinct()
            .GroupBy(p => p.Month)
            .ToDictionary(g => g.Key, g => g.Count());

        return Enumerable.Range(0, months)
            .Select(i => firstMonth.AddMonths(i).ToString("yyyy-MM"))
            .Select(m => new MonthlyActiveUsers(m, byMonth.GetValueOrDefault(m)))
            .ToList();
    }
}
