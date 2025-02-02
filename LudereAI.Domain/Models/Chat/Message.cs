using System.ComponentModel.DataAnnotations;
using LudereAI.Domain.Models.Media;
using LudereAI.Shared.Enums;

namespace LudereAI.Domain.Models.Chat;

public class Message : BaseEntity
{
    [Required]
    public string Content { get; set; }
    public MessageRole Role { get; set; }
    public int TokensUsed { get; set; }
    
    // Foreign keys
    public string ConversationId { get; set; }
    public string? ScreenshotId { get; set; }
    
    // Navigation properties
    public Conversation Conversation { get; set; }
    public Screenshot? Screenshot { get; set; }
}