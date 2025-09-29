// src/GradoCerrado.Domain/Entities/ChatMessage.cs - MANTENER COMO ESTÁ
namespace GradoCerrado.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? SessionId { get; set; }
}

public class ChatSession
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public List<ChatMessage> Messages { get; set; } = new();
}
