using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, IPasswordHasher<User> hasher, IConfiguration config) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(AuthRequest req)
    {
        var email = req.Email?.Trim().ToLowerInvariant() ?? "";
        if (email.Length == 0 || !new EmailAddressAttribute().IsValid(email))
            return BadRequest(new { error = "Invalid email." });
        if ((req.Password ?? "").Length < 8)
            return BadRequest(new { error = "Password must be at least 8 characters." });
        if (await db.Users.AnyAsync(u => u.Email == email))
            return BadRequest(new { error = "Email already registered." });

        var user = new User { Email = email, CreatedAt = DateTime.UtcNow };
        user.PasswordHash = hasher.HashPassword(user, req.Password!);
        db.Users.Add(user);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return BadRequest(new { error = "Email already registered." }); }

        return StatusCode(201, new UserResponse(user.Id, user.Email, user.CreatedAt));
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(AuthRequest req)
    {
        var email = req.Email?.Trim().ToLowerInvariant() ?? "";
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || string.IsNullOrEmpty(req.Password) ||
            hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password) == PasswordVerificationResult.Failed)
            return Unauthorized();

        var expires = DateTime.UtcNow.AddDays(7);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            claims: [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            ],
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expires));
    }
}
