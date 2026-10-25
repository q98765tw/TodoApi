namespace TodoApiGrokBot.Models;

public class LoginEvent
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
