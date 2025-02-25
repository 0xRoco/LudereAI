using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.Web.Controllers;

public class SubscriptionController : Controller
{
    
    private readonly IAPIClient _apiClient;

    public SubscriptionController(IAPIClient apiClient)
    {
        _apiClient = apiClient;
    }


    [HttpPost]
    public async Task<IActionResult> Subscribe(SubscriptionPlan plan)
    {
        var request = new SubscriptionRequestDTO
        {
            SubscriptionPlan = plan
        };
        
        var result = await _apiClient.Post<string>("Subscription/Subscribe", request);
        if (result is { IsSuccess: true, Data: not null })
        {
            var sessionUrl = result.Data;
            return Redirect(sessionUrl);
        }

        return RedirectToAction("Index", "Home");
    }
    
    [HttpPost]
    public async Task<IActionResult> CustomerPortal()
    {
        var result = await _apiClient.Get<string>("Subscription/CustomerPortal");
        if (result is { IsSuccess: true, Data: not null })
        {
            var sessionUrl = result.Data;
            return Redirect(sessionUrl);
        }

        return RedirectToAction("Index", "Home");
    }
    
    
    [HttpGet]
    public IActionResult Success()
    {
        return View();
    }
    
    [HttpGet]
    public IActionResult Cancel()
    {
        return View();
    }
}