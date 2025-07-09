using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LudereAI.Shared.Enums;

namespace LudereAI.Core.Entities.Chat;

public class Message : BaseEntity
{
    [Required]
    public string Content { get; set; } = string.Empty;

    public MessageRole Role { get; set; }
    public int TokensUsed { get; set; }
    
    // Foreign keys
    public string ConversationId { get; set; } = string.Empty;
    public string? ScreenshotId { get; set; }
    
    [NotMapped]
    public byte[] Audio { get; set; } = [];
    
    // Navigation properties
    public Conversation Conversation { get; set; } = new();
    public Screenshot? Screenshot { get; set; }
}