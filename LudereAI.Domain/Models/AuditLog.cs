namespace LudereAI.Domain.Models;

public class AuditLog : BaseEntity
{
    public string AccountId { get; set; }
    public string Action { get; set; }
    public string IPAddress { get; set; }
    
    public Account.Account Account { get; set; }
}