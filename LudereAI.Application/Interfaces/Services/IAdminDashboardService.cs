using LudereAI.Domain.Models;

namespace LudereAI.Application.Interfaces.Services;

public interface IAdminDashboardService
{
    Task<Dashboard> GetDashboardData();
}