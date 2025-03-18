using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.API.Controllers;


[ApiController, Route("[controller]")]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private readonly IAccountService _accountService;
    private readonly IAuditService _auditService;

    public AccountsController(ILogger<AccountsController> logger,
        IAccountService accountService, IAuditService auditService)
    {
        _logger = logger;
        _accountService = accountService;
        _auditService = auditService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<AccountDTO>>> GetAllAccounts()
    {
        try
        {
            var accounts = await _accountService.GetAccounts();
            
            return Ok(APIResult<IEnumerable<AccountDTO>>.Success(data: accounts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all accounts");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                APIResult<IEnumerable<AccountDTO>>.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred"));
        }
    }
    
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AccountDTO>> GetAccount([Required] string id)
    {
        try
        {
            var account = await _accountService.GetAccount(id);
            
            if (account != null) return Ok(APIResult<AccountDTO>.Success(data: account));
            
            _logger.LogWarning("Account not found for id {AccountId}", id);
            return NotFound(APIResult<AccountDTO>.Error(HttpStatusCode.NotFound, "Account not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account for id {AccountId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                APIResult<AccountDTO>.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred"));
        }
    }
    
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AccountDTO>> GetCurrentAccount()
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(accountId))
        {
            return BadRequest(APIResult<AccountDTO>.Error(HttpStatusCode.BadRequest, "Invalid token"));
        }
        
        try
        {
            
            var account = await _accountService.GetAccount(accountId);

            if (account != null)
            {
                await _auditService.Log(account.Id, "GetCurrentAccount", "Account retrieved", 
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
                return Ok(APIResult<AccountDTO>.Success(data: account));
            }
            
            return NotFound(APIResult<AccountDTO>.Error(HttpStatusCode.NotFound, "Account not found"));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account for id {AccountId}", accountId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                APIResult<AccountDTO>.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred"));
        }
    }
}