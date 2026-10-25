using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TodoApiGrokBot.Models.Dtos;
using TodoApiGrokBot.Services;

namespace TodoApiGrokBot.Tests;

public class StatsTests(TestWebAppFactory f) : IClassFixture<TestWebAppFactory>
{
    [Fact]
    public async Task Completions_returns_30_days_ending_today_with_count()
    {
        var c = await f.CreateAuthedClientAsync();
        await c.PostAsJsonAsync("/api/todos", new TodoInput("a", true));
        await c.PostAsJsonAsync("/api/todos", new TodoInput("b", true));
        await c.PostAsJsonAsync("/api/todos", new TodoInput("c", false));

        var days = (await c.GetFromJsonAsync<JsonElement[]>("/api/stats/completions"))!;
        Assert.Equal(30, days.Length);
        var today = TaipeiTime.Today(TimeProvider.System).ToString("yyyy-MM-dd");
        Assert.Equal(today, days[^1].GetProperty("date").GetString());
        Assert.Equal(2, days[^1].GetProperty("count").GetInt32());
        Assert.Equal(2, days.Sum(d => d.GetProperty("count").GetInt32()));
    }

    [Fact]
    public async Task Monthly_active_users_is_anonymous_and_returns_12_months()
    {
        await f.CreateAuthedClientAsync(); // 產生一筆登入事件
        var res = await f.CreateClient().GetAsync("/api/stats/monthly-active-users");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var months = (await res.Content.ReadFromJsonAsync<JsonElement[]>())!;
        Assert.Equal(12, months.Length);
        var thisMonth = TaipeiTime.Now(TimeProvider.System).ToString("yyyy-MM");
        Assert.Equal(thisMonth, months[^1].GetProperty("month").GetString());
        Assert.True(months[^1].GetProperty("activeUsers").GetInt32() >= 1);
        Assert.All(months, m => Assert.Equal(2, m.EnumerateObject().Count())); // 只有 month / activeUsers，不洩漏使用者資料
    }
}
