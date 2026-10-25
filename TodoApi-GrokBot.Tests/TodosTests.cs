using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TodoApiGrokBot.Models.Dtos;
using TodoApiGrokBot.Services;

namespace TodoApiGrokBot.Tests;

public class TodosTests(TestWebAppFactory f) : IClassFixture<TestWebAppFactory>
{
    private static DateOnly Today => TaipeiTime.Today(TimeProvider.System);

    private static async Task<JsonElement> CreateAsync(HttpClient c, TodoInput input)
    {
        var res = await c.PostAsJsonAsync("/api/todos", input);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        return await res.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<JsonElement[]> ListAsync(HttpClient c, string? status = null)
    {
        var res = await c.GetAsync(status is null ? "/api/todos" : $"/api/todos?status={status}");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        return (await res.Content.ReadFromJsonAsync<JsonElement[]>())!;
    }

    private static string[] Titles(JsonElement[] items) => items.Select(i => i.GetProperty("title").GetString()!).ToArray();

    [Fact]
    public async Task No_token_returns_401()
    {
        var c = f.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/todos")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.PostAsJsonAsync("/api/todos", new TodoInput("x", false))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/stats/completions")).StatusCode);
    }

    [Fact]
    public async Task Users_cannot_see_or_modify_each_others_todos()
    {
        var alice = await f.CreateAuthedClientAsync();
        var bob = await f.CreateAuthedClientAsync();
        var id = (await CreateAsync(alice, new TodoInput("alice secret", false))).GetProperty("id").GetInt32();

        Assert.Empty(await ListAsync(bob));
        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync($"/api/todos/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.PutAsJsonAsync($"/api/todos/{id}", new TodoInput("hacked", true))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.DeleteAsync($"/api/todos/{id}")).StatusCode);

        var mine = await alice.GetFromJsonAsync<JsonElement>($"/api/todos/{id}");
        Assert.Equal("alice secret", mine.GetProperty("title").GetString());
        Assert.False(mine.GetProperty("isDone").GetBoolean());
    }

    [Fact]
    public async Task Response_includes_dueDate_isOverdue_completedAt()
    {
        var c = await f.CreateAuthedClientAsync();
        var t = await CreateAsync(c, new TodoInput("late", false, Today.AddDays(-1)));
        Assert.Equal(Today.AddDays(-1).ToString("yyyy-MM-dd"), t.GetProperty("dueDate").GetString());
        Assert.True(t.GetProperty("isOverdue").GetBoolean());
        Assert.Equal(JsonValueKind.Null, t.GetProperty("completedAt").ValueKind);

        var done = await CreateAsync(c, new TodoInput("done", true));
        Assert.NotEqual(JsonValueKind.Null, done.GetProperty("completedAt").ValueKind);
        Assert.Equal(JsonValueKind.Null, done.GetProperty("dueDate").ValueKind);
    }

    [Fact]
    public async Task Status_filters_work()
    {
        var c = await f.CreateAuthedClientAsync();
        await CreateAsync(c, new TodoInput("active-nodue", false));
        await CreateAsync(c, new TodoInput("active-today", false, Today));
        await CreateAsync(c, new TodoInput("overdue", false, Today.AddDays(-3)));
        await CreateAsync(c, new TodoInput("done-pastdue", true, Today.AddDays(-3)));

        Assert.Equal(4, (await ListAsync(c)).Length);
        Assert.Equal(4, (await ListAsync(c, "all")).Length);
        Assert.Equal(["active-nodue", "active-today", "overdue"], Titles(await ListAsync(c, "active")));
        Assert.Equal(["done-pastdue"], Titles(await ListAsync(c, "completed")));
        Assert.Equal(["overdue"], Titles(await ListAsync(c, "OVERDUE")));

        var all = await ListAsync(c);
        Assert.False(all.Single(x => x.GetProperty("title").GetString() == "active-today").GetProperty("isOverdue").GetBoolean());
        Assert.False(all.Single(x => x.GetProperty("title").GetString() == "done-pastdue").GetProperty("isOverdue").GetBoolean());
    }

    [Fact]
    public async Task Invalid_status_returns_400()
    {
        var c = await f.CreateAuthedClientAsync();
        Assert.Equal(HttpStatusCode.BadRequest, (await c.GetAsync("/api/todos?status=bogus")).StatusCode);
    }

    [Fact]
    public async Task Put_replaces_whole_todo_and_clears_dueDate_and_completedAt()
    {
        var c = await f.CreateAuthedClientAsync();
        var id = (await CreateAsync(c, new TodoInput("orig", false, Today.AddDays(5)))).GetProperty("id").GetInt32();

        Assert.Equal(HttpStatusCode.NoContent, (await c.PutAsJsonAsync($"/api/todos/{id}", new TodoInput("renamed", true))).StatusCode);
        var t = await c.GetFromJsonAsync<JsonElement>($"/api/todos/{id}");
        Assert.Equal("renamed", t.GetProperty("title").GetString());
        Assert.Equal(JsonValueKind.Null, t.GetProperty("dueDate").ValueKind);
        Assert.NotEqual(JsonValueKind.Null, t.GetProperty("completedAt").ValueKind);

        Assert.Equal(HttpStatusCode.NoContent, (await c.PutAsJsonAsync($"/api/todos/{id}", new TodoInput("reopened", false))).StatusCode);
        t = await c.GetFromJsonAsync<JsonElement>($"/api/todos/{id}");
        Assert.Equal(JsonValueKind.Null, t.GetProperty("completedAt").ValueKind);
    }

    [Fact]
    public async Task Delete_then_get_returns_404()
    {
        var c = await f.CreateAuthedClientAsync();
        var id = (await CreateAsync(c, new TodoInput("tmp", false))).GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.NoContent, (await c.DeleteAsync($"/api/todos/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync($"/api/todos/{id}")).StatusCode);
    }
}
