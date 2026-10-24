namespace TodoApi.Models.Dtos;

public record AuthRequest(string Email, string Password);
public record UserResponse(int Id, string Email, DateTime CreatedAt);
public record LoginResponse(string Token, DateTime ExpiresAt);
