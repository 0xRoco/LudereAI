using LudereAI.Domain.Models;

namespace LudereAI.Web.Models.Admin;

public class DashboardViewModel
{
    public int TotalUsers { get; set; }
    public List<AuditLog> AuditLogs { get; set; } = [];
}