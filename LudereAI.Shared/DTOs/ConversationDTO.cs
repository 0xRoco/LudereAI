namespace LudereAI.Shared.DTOs;

public class ConversationDTO
{
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public IEnumerable<MessageDTO> Messages { get; set; } = new List<MessageDTO>();
}