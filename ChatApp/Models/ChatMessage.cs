namespace ChatApp.Models;

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Guid ChatSessionId { get; set; }
    public ChatSession ChatSession { get; set; } = null!;
}