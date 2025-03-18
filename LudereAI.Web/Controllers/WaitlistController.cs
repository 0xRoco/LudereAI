using LudereAI.Application.Interfaces.Gateways;
using LudereAI.Web.Models.Waitlist;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.Web.Controllers;

public class WaitlistController : Controller
{
    private IWaitlistGateway _waitlistGateway;

    public WaitlistController(IWaitlistGateway waitlistGateway)
    {
        _waitlistGateway = waitlistGateway;
    }

    public IActionResult Index()
    {
        return View(new JoinWaitlistViewModel());
    }
    
    [HttpPost]
    public async Task<IActionResult> Index(JoinWaitlistViewModel vm)
    {
        var result = await _waitlistGateway.JoinWaitlist(vm.Email);
        
        if (string.IsNullOrWhiteSpace(result.Email))
        {
            return BadRequest();
        }
        
        return RedirectToAction("Success", new JoinedWaitlistViewModel
        {
            Email = result.Email,
            Position = result.Position
        });
    }

    public IActionResult Success(JoinedWaitlistViewModel vm)
    {
        return View(vm);
    }
}