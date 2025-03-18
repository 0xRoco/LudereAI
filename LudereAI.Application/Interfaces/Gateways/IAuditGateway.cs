using LudereAI.Domain.Models;

namespace LudereAI.Application.Interfaces.Gateways;

public interface IAuditGateway
{
    Task<IEnumerable<AuditLog>> GetLogs();
}