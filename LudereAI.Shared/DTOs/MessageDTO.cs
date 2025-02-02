using LudereAI.Shared.Enums;

namespace LudereAI.Shared.DTOs;

public class MessageDTO
{
    public string Id { get; set; }
    public string ConversationId { get; set; }
    public string Content { get; set; }
    public byte[] Audio { get; set; }
    public MessageRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public MessageDTO(){}
}