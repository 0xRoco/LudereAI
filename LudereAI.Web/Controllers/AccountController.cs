using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared.DTOs;
using LudereAI.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.Web.Controllers;

public class AccountController : Controller
{
    
    private readonly IAPIClient _apiClient;

    public AccountController(IAPIClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _apiClient.GetAsync<AccountDTO>("Accounts/me");
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