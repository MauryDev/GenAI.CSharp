using System;
using System.Collections.Generic;

namespace ChatApp.Models;

public class ChatSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "Novo Chat";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ChatMessage> Messages { get; set; } = new();
}