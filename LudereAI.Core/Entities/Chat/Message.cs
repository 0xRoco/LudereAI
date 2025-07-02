using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LudereAI.Core.Entities.Media;
using LudereAI.Shared.Enums;

namespace LudereAI.Core.Entities.Chat;

public class Message : BaseEntity
{
    [Required]
    public string Content { get; set; }
    public MessageRole Role { get; set; }
    public int TokensUsed { get; set; }
    
    // Foreign keys
    public string ConversationId { get; set; }
    public string? ScreenshotId { get; set; }
    
    [NotMapped]
    public byte[] Audio { get; set; }
    
    // Navigation properties
    public Conversation Conversation { get; set; }
    public Screenshot? Screenshot { get; set; }
}