namespace LudereAI.Core.Entities.Chat;

public class Conversation : BaseEntity
{ 
    public string GameContext { get; set; } = string.Empty;
    public IEnumerable<Message> Messages { get; set; } = new List<Message>();
}