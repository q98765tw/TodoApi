using Microsoft.Extensions.Time.Testing;
using TodoApiGrokBot.Models;
using TodoApiGrokBot.Models.Dtos;
using TodoApiGrokBot.Services;

namespace TodoApiGrokBot.Tests;

public class TaipeiTimeTests
{
    [Theory]
    [InlineData("2026-10-25T15:59:59Z", "2026-10-25")] // 台北 23:59:59
    [InlineData("2026-10-25T16:00:00Z", "2026-10-26")] // 台北 00:00 換日
    [InlineData("2026-12-31T16:00:00Z", "2027-01-01")] // 跨年
    public void Today_uses_utc_plus_8(string utc, string expected)
    {
        var clock = new FakeTimeProvider(DateTimeOffset.Parse(utc));
        Assert.Equal(DateOnly.Parse(expected), TaipeiTime.Today(clock));
    }

    [Fact]
    public void ToUtc_subtracts_8_hours()
    {
        var utc = TaipeiTime.ToUtc(new DateTime(2026, 10, 26, 0, 0, 0));
        Assert.Equal(new DateTime(2026, 10, 25, 16, 0, 0), utc);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
    }
}

public class OverdueLogicTests
{
    private static DateOnly Today => TaipeiTime.Today(TimeProvider.System);

    [Fact] public void Past_due_and_not_done_is_overdue() => Assert.True(new TodoItem { DueDate = Today.AddDays(-1) }.IsOverdue);
    [Fact] public void Due_today_is_not_overdue() => Assert.False(new TodoItem { DueDate = Today }.IsOverdue);
    [Fact] public void Done_is_never_overdue() => Assert.False(new TodoItem { DueDate = Today.AddDays(-1), IsDone = true }.IsOverdue);
    [Fact] public void No_due_date_is_not_overdue() => Assert.False(new TodoItem().IsOverdue);

    [Theory]
    [InlineData(null, TodoStatusFilter.All, true)]
    [InlineData(" Active ", TodoStatusFilter.Active, true)]
    [InlineData("completed", TodoStatusFilter.Completed, true)]
    [InlineData("overdue", TodoStatusFilter.Overdue, true)]
    [InlineData("done", TodoStatusFilter.All, false)]
    public void Status_parser(string? input, TodoStatusFilter expected, bool ok)
    {
        Assert.Equal(ok, TodoStatusFilterParser.TryParse(input, out var f));
        Assert.Equal(expected, f);
    }
}
