using LudereAI.Shared.DTOs;

namespace LudereAI.Domain.DTOs;

public class AuditLogDTO
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Details { get; set; } = string.Empty;
}