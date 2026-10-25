using System.Net;
using System.Net.Http.Json;
using TodoApiGrokBot.Models.Dtos;

namespace TodoApiGrokBot.Tests;

public class AuthTests(TestWebAppFactory f) : IClassFixture<TestWebAppFactory>
{
    private static string NewEmail() => $"u{Guid.NewGuid():N}@example.com";

    [Fact]
    public async Task Register_then_login_returns_token()
    {
        var c = f.CreateClient();
        var email = NewEmail();
        var reg = await c.PostAsJsonAsync("/api/auth/register", new AuthRequest(email, "Passw0rd!"));
        Assert.Equal(HttpStatusCode.Created, reg.StatusCode);
        var login = await c.PostAsJsonAsync("/api/auth/login", new AuthRequest(email, "Passw0rd!"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var body = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(body!.Token));
        Assert.True(body.ExpiresAt > DateTime.UtcNow);
    }

    [Theory]
    [InlineData("not-an-email", "Passw0rd!")]
    [InlineData("", "Passw0rd!")]
    [InlineData("valid@example.com", "short")]
    public async Task Register_invalid_returns_400(string email, string password)
    {
        var res = await f.CreateClient().PostAsJsonAsync("/api/auth/register", new AuthRequest(email, password));
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Register_duplicate_email_returns_400()
    {
        var c = f.CreateClient();
        var email = NewEmail();
        Assert.Equal(HttpStatusCode.Created, (await c.PostAsJsonAsync("/api/auth/register", new AuthRequest(email, "Passw0rd!"))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/auth/register", new AuthRequest(email, "Passw0rd!"))).StatusCode);
    }

    [Fact]
    public async Task Login_wrong_password_or_unknown_user_returns_401()
    {
        var c = f.CreateClient();
        var email = NewEmail();
        await c.PostAsJsonAsync("/api/auth/register", new AuthRequest(email, "Passw0rd!"));
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.PostAsJsonAsync("/api/auth/login", new AuthRequest(email, "WrongPass1"))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.PostAsJsonAsync("/api/auth/login", new AuthRequest(NewEmail(), "Passw0rd!"))).StatusCode);
    }
}
