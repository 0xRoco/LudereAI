using Microsoft.AspNetCore.Mvc;

namespace LudereAI.Web.Controllers;

public class WaitlistController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}