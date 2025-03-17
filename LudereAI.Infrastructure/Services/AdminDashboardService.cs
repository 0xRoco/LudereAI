using LudereAI.Application.Interfaces.Gateways;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly ILogger<IAdminDashboardService> _logger;
    private readonly IAccountGateway _accountGateway;
    private readonly IAuditGateway _auditGateway;
    
    public AdminDashboardService(ILogger<IAdminDashboardService> logger, IAccountGateway accountGateway, IAuditGateway auditGateway)
    {
        _logger = logger;
        _accountGateway = accountGateway;
        _auditGateway = auditGateway;
    }
    
    public async Task<Dashboard> GetDashboardData()
    {
        try
        {
            _logger.LogInformation("Fetching dashboard data");

            var dashboard = new Dashboard();

            var accountsResult = await _accountGateway.GetAccounts();
            var accounts = accountsResult.Value;
            if (accounts != null)
            {
                dashboard.TotalUsers = accounts.Count();
            }
            
            var auditLogs = await _auditGateway.GetLogs();
            dashboard.AuditLogs = auditLogs.Take(5).ToList();
            

            _logger.LogInformation("Dashboard data fetched successfully");
            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard data");
            throw new Exception("Error fetching dashboard data", ex);
        }
    }
}