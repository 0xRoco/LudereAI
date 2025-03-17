using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Services;

public class AuditService : IAuditService
{
    private ILogger<IAuditService> _logger;
    private DatabaseContext _context;


    public AuditService(ILogger<IAuditService> logger, DatabaseContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IEnumerable<AuditLog>> GetLogs()
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Include(a => a.Account)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetLogs(string accountId)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .Include(a => a.Account)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetLogs(string accountId, string action)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(x => x.AccountId == accountId && x.Action == action)
            .Include(a => a.Account)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task Log(string accountId, string action, string message, string ipAddress)
    {
        var log = new AuditLog
        {
            AccountId = accountId,
            Action = action,
            Message = message,
            IPAddress = ipAddress,
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}