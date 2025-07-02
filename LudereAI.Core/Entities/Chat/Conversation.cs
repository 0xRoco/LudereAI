namespace LudereAI.Core.Entities.Chat;

public class Conversation : BaseEntity
{ 
    public string GameContext { get; set; }
    public IEnumerable<Message> Messages { get; set; } = new List<Message>();
}