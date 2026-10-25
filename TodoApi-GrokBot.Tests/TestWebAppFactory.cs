using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TodoApiGrokBot.Data;
using TodoApiGrokBot.Models.Dtos;

namespace TodoApiGrokBot.Tests;

/// <summary>用 SQLite in-memory 取代 PostgreSQL；JWT 金鑰為測試專用假值，不碰任何真實秘密。</summary>
public class TestWebAppFactory : WebApplicationFactory<Program>
{
    public const string FakeJwtKey = "test-only-fake-jwt-key-0123456789-abcdefghijklmnop";
    private readonly SqliteConnection _conn = new("DataSource=:memory:");

    public TestWebAppFactory() => _conn.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Key", FakeJwtKey);
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=invalid.test;Database=none");
        builder.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = FakeJwtKey,
            ["ConnectionStrings:DefaultConnection"] = "Host=invalid.test;Database=none",
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_conn));
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }

    public async Task<HttpClient> CreateAuthedClientAsync(string? email = null, string password = "Passw0rd!")
    {
        email ??= $"u{Guid.NewGuid():N}@example.com";
        var c = CreateClient();
        (await c.PostAsJsonAsync("/api/auth/register", new AuthRequest(email, password))).EnsureSuccessStatusCode();
        var res = await c.PostAsJsonAsync("/api/auth/login", new AuthRequest(email, password));
        res.EnsureSuccessStatusCode();
        var login = await res.Content.ReadFromJsonAsync<LoginResponse>();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
        return c;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _conn.Dispose();
    }
}
