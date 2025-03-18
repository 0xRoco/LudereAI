using LudereAI.Domain.Models;

namespace LudereAI.Web.Models.Admin;

public class WaitlistViewModel
{
    public IEnumerable<WaitlistEntry> Entries { get; set; } = new List<WaitlistEntry>();
    public int TotalEntries => Entries.Count();
    public int InvitedCount => Entries.Count(e => e.IsInvited);
    public int PendingCount => Entries.Count(e => !e.IsInvited);
    public string? StatusMessage { get; set; } = "Total Entries: 0";
    public string? ErrorMessage { get; set; } = "Total Entries: 0";
    public int BatchSize { get; set; } = 10;
    public string? EmailToInvite { get; set; }
    public string? EmailToRemove { get; set; }
}