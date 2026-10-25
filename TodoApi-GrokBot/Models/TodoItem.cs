using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TodoApiGrokBot.Services;

namespace TodoApiGrokBot.Models;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    /// <summary>截止日（台灣日曆日期，無時間）；null = 沒有期限。</summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>未完成且截止日早於台北今天（今天到期不算逾期）。</summary>
    [NotMapped]
    public bool IsOverdue => !IsDone && DueDate is { } d && d < TaipeiTime.Today(TimeProvider.System);

    [JsonIgnore]
    public int UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
}
