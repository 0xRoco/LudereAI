using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared.DTOs;
using LudereAI.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.Web.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly IAPIClient _apiClient;

    public AccountController(IAPIClient apiClient, ILogger<AccountController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _apiClient.Get<AccountDTO>("Accounts/me");
        if (result?.Data is null || !result.IsSuccess)
        {
            return RedirectToAction("Login", "Auth");
        }
        
        return View(new AccountViewModel
        {
            Account = result.Data
        });
    }


    
}