using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoApiGrokBot.Data;
using TodoApiGrokBot.Models;
using TodoApiGrokBot.Models.Dtos;

namespace TodoApiGrokBot.Services;

public class AuthService(AppDbContext db, IPasswordHasher<User> hasher, ITokenService tokens) : IAuthService
{
    private const string EmailTaken = "Email already registered.";

    private static string Normalize(string? email) => email?.Trim().ToLowerInvariant() ?? "";

    public async Task<RegisterResult> RegisterAsync(AuthRequest req)
    {
        var email = Normalize(req.Email);
        if (email.Length == 0 || !new EmailAddressAttribute().IsValid(email))
            return RegisterResult.Fail("Invalid email.");
        if ((req.Password ?? "").Length < 8)
            return RegisterResult.Fail("Password must be at least 8 characters.");
        if (await db.Users.AnyAsync(u => u.Email == email))
            return RegisterResult.Fail(EmailTaken);

        var user = new User { Email = email, CreatedAt = DateTime.UtcNow };
        user.PasswordHash = hasher.HashPassword(user, req.Password!);
        db.Users.Add(user);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return RegisterResult.Fail(EmailTaken); }

        return RegisterResult.Ok(new UserResponse(user.Id, user.Email, user.CreatedAt));
    }

    public async Task<LoginResponse?> LoginAsync(AuthRequest req)
    {
        var email = Normalize(req.Email);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || string.IsNullOrEmpty(req.Password) ||
            hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password) == PasswordVerificationResult.Failed)
            return null;
        return tokens.CreateToken(user);
    }
}
