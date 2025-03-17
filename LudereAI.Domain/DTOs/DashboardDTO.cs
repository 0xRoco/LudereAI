namespace LudereAI.Domain.DTOs;

public class DashboardDTO
{
    public int TotalUsers { get; set; }
    public IEnumerable<AuditLogDTO> AuditLogs { get; set; } = new List<AuditLogDTO>();
}