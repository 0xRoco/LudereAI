using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.Web.Controllers.Admin;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly IAdminDashboardService _dashboardService;
    
    public AdminController(ILogger<AdminController> logger, IAdminDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }
    
    public async Task<IActionResult> Index()
    {
        var dashboardData = await _dashboardService.GetDashboardData();
        var viewModel = new DashboardViewModel
        {
            TotalUsers = dashboardData.TotalUsers,
            AuditLogs = dashboardData.AuditLogs
        };
        return View(viewModel);
    }
    
    public IActionResult Users()
    {
        return View();
    }
    
    public IActionResult Waitlist()
    {
        var viewModel = new WaitlistViewModel
        {
            Entries = new List<WaitlistEntry>()
            {
                new()
                {
                    Position = 1,
                    Email = "test"
                },
                new()
                {
                    Position = 2,
                    Email = "test2"
                }
            },
            StatusMessage = null,
            ErrorMessage = null,
            BatchSize = 10,
            EmailToInvite = null,
            EmailToRemove = null
        };
        return View(viewModel);
    }
}