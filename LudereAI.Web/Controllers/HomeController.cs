using System.Diagnostics;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models.Configs;
using Microsoft.AspNetCore.Mvc;
using LudereAI.Web.Models;
using Microsoft.AspNetCore.Authorization;

namespace LudereAI.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAPIClient _apiClient;
    private readonly HomeViewModel _viewModel;

    public HomeController(ILogger<HomeController> logger, IAPIClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
        
        _viewModel = new HomeViewModel();
    }

    public IActionResult Index()
    {
        return View();
    }
    
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}