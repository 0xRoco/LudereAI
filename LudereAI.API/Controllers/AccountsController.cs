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
public class AccountsController(ILogger<AccountsController> logger,
    IAccountService accountService) : ControllerBase
{
    
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AccountDTO>> GetAccount([Required] string id)
    {
        try
        {
            var account = await accountService.GetAccount(id);
            
            if (account != null) return Ok(APIResult<AccountDTO>.Success(data: account));
            
            logger.LogWarning("Account not found for id {AccountId}", id);
            return NotFound(APIResult<AccountDTO>.Error(HttpStatusCode.NotFound, "Account not found"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting account for id {AccountId}", id);
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
            
            var account = await accountService.GetAccount(accountId);
            
            if (account != null) return Ok(APIResult<AccountDTO>.Success(data: account));
            
            return NotFound(APIResult<AccountDTO>.Error(HttpStatusCode.NotFound, "Account not found"));

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting account for id {AccountId}", accountId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                APIResult<AccountDTO>.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred"));
        }
    }
}