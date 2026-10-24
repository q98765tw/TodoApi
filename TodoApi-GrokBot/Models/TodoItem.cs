using System.Text.Json.Serialization;

namespace TodoApiGrokBot.Models;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public int UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
}
