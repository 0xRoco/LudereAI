namespace LudereAI.Domain.Models;

public class WaitlistEntry : BaseEntity
{
    public string Email { get; set; } = string.Empty; 
    public DateTime JoinedDate { get; set; }
    public int Position { get; set; }
    public bool IsInvited { get; set; }
    public DateTime? InvitedDate { get; set; }
    public DateTime? RegisteredDate { get; set; }
}