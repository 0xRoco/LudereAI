namespace LudereAI.Domain.Models.Account;

public class AccountUsage : BaseEntity
{
    public string AccountId { get; set; }
    public DateTime Date { get; set; }
    public int MessageCount { get; set; }
    public int ScreenshotCount { get; set; }
    
    public Account Account { get; set; }
}