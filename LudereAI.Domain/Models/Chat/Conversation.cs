namespace LudereAI.Domain.Models.Chat;

public class Conversation : BaseEntity
{ 
    public string AccountId { get; set; }
    
    public Account.Account Account { get; set; }
    public IEnumerable<Message> Messages { get; set; } = new List<Message>();
}