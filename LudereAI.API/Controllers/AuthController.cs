using System.Net;
using System.Security.Claims;
using LudereAI.API.Core;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.API.Controllers;


[ApiController, Route("api/[controller]")]
public class AuthController(ILogger<AuthController> logger, IAuthService authService, IAccountService accountService) : ControllerBase
{

    [RequireFeature("Auth.LoginEnabled")]
    [HttpPost("[action]")]
    public async Task<ActionResult<APIResult<LoginResponseDTO>>> Login([FromBody] LoginDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(APIResult<LoginResponseDTO>.Error(HttpStatusCode.BadRequest, "Username and password cannot be empty"));
        }
        
        var loggedIn = await authService.LoginAsync(dto);
        if (!loggedIn)
        {
            return BadRequest(APIResult<LoginResponseDTO>.Error(HttpStatusCode.Unauthorized, "Invalid username or password"));
        }

        var account = await accountService.GetAccountByUsername(dto.Username);
        if (account == null)
        {
            return BadRequest(APIResult<LoginResponseDTO>.Error(HttpStatusCode.BadRequest, "Account not found"));
        }
        
        var token = authService.GenerateJWT(account);
        
        var response = new LoginResponseDTO
        {
            Token = token,
            Account = account
        };
        
        return Ok(APIResult<LoginResponseDTO>.Success(data: response));
    }

    [RequireFeature("Auth.SignUpEnabled")]
    [HttpPost("[action]")]
    public async Task<ActionResult<APIResult<LoginResponseDTO>>> SignUp([FromBody] SignUpDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Username, email and password cannot be empty"));
        }

        var registered = await authService.RegisterAsync(dto);
        if (!registered)
        {
            return BadRequest(APIResult<string>.Error(HttpStatusCode.BadRequest, "Username or email already exists"));
        }

        return Ok(APIResult<string>.Success(data: "Account created successfully"));
    }
    
    [HttpPost("[action]")]
    public async Task<ActionResult<APIResult<bool>>> Logout()
    {
        /*
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(accountId)) return BadRequest(APIResult<bool>.Error(HttpStatusCode.Unauthorized, "Invalid token"));
        var account = await accountRepository.GetAsync(accountId);
        if (account == null) return BadRequest(APIResult<bool>.Error(HttpStatusCode.Unauthorized, "Account not found"));
        */
        
        return Ok(APIResult<bool>.Success(data: true));
    }
    
    [HttpGet("[action]"), Authorize]
    public async Task<ActionResult<APIResult<bool>>> ValidateToken()
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(accountId)) return BadRequest(APIResult<bool>.Error(HttpStatusCode.Unauthorized, "Invalid token"));
        var account = await accountService.GetAccount(accountId);
        if (account == null) return BadRequest(APIResult<bool>.Error(HttpStatusCode.Unauthorized, "Account not found"));
        
        return Ok(APIResult<bool>.Success(data: true));
    }
}