namespace TodoApiGrokBot.Models.Dtos;

/// <summary>DueDate 省略或 null = 沒有期限（PUT 時會清除期限）。</summary>
public record TodoInput(string Title, bool IsDone, DateOnly? DueDate = null);

public enum TodoStatusFilter { All, Active, Completed, Overdue }

public static class TodoStatusFilterParser
{
    public static bool TryParse(string? value, out TodoStatusFilter filter)
    {
        filter = TodoStatusFilter.All;
        switch ((value ?? "all").Trim().ToLowerInvariant())
        {
            case "all": return true;
            case "active": filter = TodoStatusFilter.Active; return true;
            case "completed": filter = TodoStatusFilter.Completed; return true;
            case "overdue": filter = TodoStatusFilter.Overdue; return true;
            default: return false;
        }
    }
}
