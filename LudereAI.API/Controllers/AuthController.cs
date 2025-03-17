using System.Net;
using System.Security.Claims;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.API.Controllers;


[ApiController, Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthService _authService;
    private readonly IAccountService _accountService;
    private readonly IGuestService _guestService;
    
    public AuthController(ILogger<AuthController> logger, IAuthService authService, IAccountService accountService, IGuestService guestService)
    {
        _logger = logger;
        _authService = authService;
        _accountService = accountService;
        _guestService = guestService;
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<APIResult<LoginResponseDTO>>> Login([FromBody] LoginDTO dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(APIResult<LoginResponseDTO>.Error(HttpStatusCode.BadRequest,
                    "Username and password cannot be empty"));
            }
            var account = await _accountService.GetAccountByUsername(dto.Username);
            if (account == null)
            {
                return BadRequest(APIResult<LoginResponseDTO>.Error(HttpStatusCode.BadRequest, "Account not found"));
            }
            

            var loggedIn = await _authService.LoginAsync(dto);
            if (!loggedIn)
            {
                return BadRequest(APIResult<LoginResponseDTO>.Error(HttpStatusCode.Unauthorized,
                    "Invalid username or password"));
            }
            
            
            var token = _authService.GenerateJWT(account);

            var response = new LoginResponseDTO
            {
                Token = token,
                Account = account
            };
            
            await _accountService.UpdateLastLogin(account.Id);
            return Ok(APIResult<LoginResponseDTO>.Success(data: response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to login");
            return BadRequest(APIResult<LoginResponseDTO>.Error(HttpStatusCode.BadRequest, "Failed to login"));
        }
    }
    
    [HttpPost("[action]")]
    public async Task<ActionResult<APIResult<LoginResponseDTO>>> GuestLogin([FromBody] GuestDTO dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.DeviceId))
            {
                return BadRequest(
                    APIResult<LoginResponseDTO>.Error(HttpStatusCode.BadRequest, "DeviceId cannot be empty"));
            }
            
            
            var account = await _guestService.CreateGuestAccount(dto);

            if (account == null)
            {
                return BadRequest(APIResult<LoginResponseDTO>.Error(HttpStatusCode.BadRequest,
                    "Failed to create guest account"));
            }
            
            var token = _authService.GenerateGuestJWT(account);

            var response = new LoginResponseDTO
            {
                Token = token,
                Account = new AccountDTO
                {
                    Id = account.Id,
                    Username = account.Username,
                    Email = "guest@LudereAI.com",
                    FirstName = "Guest",
                    LastName = "User",
                    DeviceId = account.DeviceId,
                    Role = AccountRole.User,
                    Status = AccountStatus.Active,
                    CreatedAt = account.CreatedAt,
                }
            };
            
            await _accountService.UpdateLastLogin(account.Id);
            return Ok(APIResult<LoginResponseDTO>.Success(data: response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create guest account");
            return BadRequest(APIResult<LoginResponseDTO>.Error(HttpStatusCode.BadRequest, "Failed to create guest account"));
        }
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<APIResult<bool>>> SignUp([FromBody] SignUpDTO dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(APIResult<bool>.Error(HttpStatusCode.BadRequest, "Username, email and password cannot be empty"));
            }

            var registered = await _authService.RegisterAsync(dto);
            if (!registered)
            {
                return BadRequest(APIResult<bool>.Error(HttpStatusCode.BadRequest, "Username or email already exists"));
            }
            
            var account = await _accountService.GetAccountByUsername(dto.Username);
            
            return Ok(APIResult<bool>.Success(data: true));
        }catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create account");
            return BadRequest(APIResult<bool>.Error(HttpStatusCode.BadRequest, "Failed to create account"));
        }
    }
    
    [HttpPost("[action]")]
    public async Task<ActionResult<APIResult<bool>>> Logout()
    {
        
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(accountId)) return BadRequest(APIResult<bool>.Error(HttpStatusCode.Unauthorized, "Invalid token"));
        var account = await _accountService.GetAccount(accountId);
        if (account == null) return BadRequest(APIResult<bool>.Error(HttpStatusCode.Unauthorized, "Account not found"));
        
        return Ok(APIResult<bool>.Success(data: true));
    }
    
    [HttpGet("[action]"), Authorize]
    public async Task<ActionResult<APIResult<AccountDTO>>> ValidateToken()
    {
        try
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrWhiteSpace(accountId)) return BadRequest(APIResult<AccountDTO>.Error(HttpStatusCode.Unauthorized, "Invalid token"));
            
            if (!Enum.TryParse<AccountRole>(role, out _))
            {
                return BadRequest(APIResult<AccountDTO>.Error(HttpStatusCode.Unauthorized, "Invalid role"));
            }
            
            var account = await _accountService.GetAccount(accountId);
            
            if (account == null) return BadRequest(APIResult<AccountDTO>.Error(HttpStatusCode.Unauthorized, "Account not found"));
            
            return Ok(APIResult<AccountDTO>.Success(data: account));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate token");
            return BadRequest(APIResult<AccountDTO>.Error(HttpStatusCode.BadRequest, "Failed to validate token"));
        }
    }
}