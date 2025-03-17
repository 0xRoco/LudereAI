namespace LudereAI.Domain.Models;

public class Dashboard
{
    public int TotalUsers { get; set; }
    public List<AuditLog> AuditLogs { get; set; } = [];
}