namespace LudereAI.Domain.DTOs;

public class WaitlistEntryDTO
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Position { get; set; }
    public DateTime JoinedDate { get; set; }
    public bool IsInvited { get; set; }
    public DateTime? InvitedDate { get; set; }
}