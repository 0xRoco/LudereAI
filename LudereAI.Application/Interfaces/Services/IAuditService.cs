using LudereAI.Domain.Models;

namespace LudereAI.Application.Interfaces.Services;

public interface IAuditService
{
    Task<IEnumerable<AuditLog>> GetLogs();
    Task<IEnumerable<AuditLog>> GetLogs(string accountId);
    Task<IEnumerable<AuditLog>> GetLogs(string accountId, string action);
    
    Task Log(string accountId, string action, string message, string ipAddress);
}