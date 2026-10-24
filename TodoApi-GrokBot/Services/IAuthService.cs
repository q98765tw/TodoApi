using TodoApiGrokBot.Models.Dtos;

namespace TodoApiGrokBot.Services;

public record RegisterResult(UserResponse? User, string? Error)
{
    public bool Succeeded => User is not null;
    public static RegisterResult Ok(UserResponse user) => new(user, null);
    public static RegisterResult Fail(string error) => new(null, error);
}

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(AuthRequest req);
    /// <summary>帳密正確回傳 token，否則 null。</summary>
    Task<LoginResponse?> LoginAsync(AuthRequest req);
}
