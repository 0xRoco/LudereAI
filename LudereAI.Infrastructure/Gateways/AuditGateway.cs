using LudereAI.Application.Interfaces.Gateways;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Gateways;

public class AuditGateway : IAuditGateway
{
    private readonly IAPIClient _apiClient;
    private readonly ILogger<AuditGateway> _logger;
    
    public AuditGateway(IAPIClient apiClient, ILogger<AuditGateway> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }
    
    
    public async Task<IEnumerable<AuditLog>> GetLogs()
    {
        try
        {
            var response = await _apiClient.Get<IEnumerable<AuditLog>>("audits/");
            if (response is {IsSuccess: true, Data: not null })
            {
                var logs = response.Data;
                return logs;
            }
            else
            {
                _logger.LogError($"Failed to fetch audit logs. Status code: {response?.StatusCode}");
                return [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit logs");
            throw;
        }
    }
}