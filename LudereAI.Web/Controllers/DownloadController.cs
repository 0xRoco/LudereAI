using System.Diagnostics;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models.Configs;
using Microsoft.AspNetCore.Mvc;
using LudereAI.Web.Models;

namespace LudereAI.Web.Controllers;

public class DownloadController : Controller
{

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