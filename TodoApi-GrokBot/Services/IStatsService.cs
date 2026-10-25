using TodoApiGrokBot.Models.Dtos;

namespace TodoApiGrokBot.Services;

/// <summary>統計一律以台北時間（固定 UTC+8，台灣無日光節約）切日 / 切月。</summary>
public interface IStatsService
{
    /// <summary>最近 30 天（含今天）每天完成的 Todo 數，補零。</summary>
    Task<List<DailyCount>> GetCompletionsAsync(int userId, int days = 30);
    /// <summary>最近 12 個月（含本月）每月登入過的不重複使用者數，補零。</summary>
    Task<List<MonthlyActiveUsers>> GetMonthlyActiveUsersAsync(int months = 12);
}
