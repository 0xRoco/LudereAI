using System.Security.Claims;
using LudereAI.Application.Interfaces.Gateways;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared.DTOs;
using LudereAI.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.Web.Controllers;

[Route("[action]")]
public class AuthController : Controller
{
    
    private readonly IAuthGateway _authGateway;

    public AuthController(IAuthGateway authGateway)
    {
        _authGateway = authGateway;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        
        return View(new LoginViewModel());
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var result = await _authGateway.LoginAsync(new LoginDTO
        {
            Username = model.Username,
            Password = model.Password
        });
        
        if (result.Value == null || string.IsNullOrWhiteSpace(result.Value.Token))
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        var response = result.Value;
        var account = response.Account;
        
        SetAuthCookie(response.Token, DateTimeOffset.UtcNow.AddHours(3));
        await SignInUser(account, model.RememberMe);
        
        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet]
    public IActionResult SignUp()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        
        return View(new SignUpViewModel());
    }
    
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignUp(SignUpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Invalid sign up attempt");
            return View(model);
        }
        
        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("Password", "Passwords do not match");
            return View(model);
        }

        var dto = new SignUpDTO
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Username = model.Username,
            Email = model.Email,
            Password = model.Password,
            DeviceId = ""
        };

        var result = await _authGateway.SignUpAsync(dto);

        var success = result is { IsSuccessful: true, Value: true };

        if (success) return RedirectToAction("Login");
        
        
        ModelState.AddModelError(string.Empty, result.Message);
        return View(model);
    }
    
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        Response.Cookies.Delete("token");
        return Challenge();
    }
    
    
    private void SetAuthCookie(string token, DateTimeOffset expiration)
    {
        Response.Cookies.Append("token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiration,
        });
    }
    
    private async Task SignInUser(AccountDTO account, bool isPersistent = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id),
            new(ClaimTypes.Name, account.Username),
            new(ClaimTypes.Role, account.Role.ToString())
        };

        var principal =
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties
            {
                IsPersistent = isPersistent
            });
    }
}