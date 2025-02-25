using System.ComponentModel.DataAnnotations;
using System.Net;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.DTOs.Waitlist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.API.Controllers;


[ApiController, Route("[controller]")]
public class WaitlistController : ControllerBase
{
    private readonly ILogger<WaitlistController> _logger;
    private readonly IWaitlistService _waitlistService;

    public WaitlistController(ILogger<WaitlistController> logger, IWaitlistService waitlistService)
    {
        _logger = logger;
        _waitlistService = waitlistService;
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<APIResult<IEnumerable<WaitlistEntry>>>> GetAll()
    {
        try
        {
            var result = await _waitlistService.GetAll();
            
            return Ok(APIResult<IEnumerable<WaitlistEntry>>.Success(data: result.Select(x => new WaitlistEntry
            {
                Email = x.Email,
                JoinedDate = x.JoinedDate,
                Position = x.Position,
                IsInvited = x.IsInvited,
                InvitedDate = x.InvitedDate,
            })));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get waitlist entries");
            return BadRequest(APIResult<IEnumerable<WaitlistEntry>>.Error(HttpStatusCode.InternalServerError, ex.Message));
        }
    }
    
    [HttpGet("GetByEmail")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<APIResult<WaitlistEntry>>> Get([Required, EmailAddress] string email)
    {
        try
        {
            var result = await _waitlistService.GetByEmail(email);

            return Ok(result is null 
                ? APIResult<WaitlistEntry>.Error(HttpStatusCode.NotFound, "Waitlist entry not found") 
                : APIResult<WaitlistEntry>.Success(data: result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get waitlist entry");
            return BadRequest(APIResult<WaitlistEntry>.Error(HttpStatusCode.InternalServerError, ex.Message));
        }
    }
    
    [HttpPost("Join")]
    public async Task<ActionResult<APIResult<JoinedWaitlistDTO>>> Join([Required, FromBody] JoinWaitlistDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(APIResult<JoinedWaitlistDTO>.Error(HttpStatusCode.BadRequest, "Invalid request"));
            }
            
            var result = await _waitlistService.JoinWaitlist(dto.Email);
            
            return Ok(APIResult<JoinedWaitlistDTO>.Success(data: new JoinedWaitlistDTO
            {
                Email = result.Email,
                Position = result.Position,
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add email to waitlist");
            return BadRequest(APIResult<bool>.Error(HttpStatusCode.InternalServerError, ex.Message));
        }
    }
    
    [HttpPost("Invite")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<APIResult<bool>>> Invite([Required, EmailAddress] string email)
    {
        try
        {
            var result = await _waitlistService.Invite(email);
            
            return Ok(APIResult<bool>.Success(data: result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invite email from waitlist");
            return BadRequest(APIResult<bool>.Error(HttpStatusCode.InternalServerError, ex.Message));
        }
    }
    
    [HttpPost("InviteBatch")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<APIResult<bool>>> InviteBatch([Required, Range(1, 100)] int batchSize)
    {
        try
        {
            var result = await _waitlistService.InviteNextBatch(batchSize);
            
            return Ok(APIResult<bool>.Success(data: result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invite next batch from waitlist");
            return BadRequest(APIResult<bool>.Error(HttpStatusCode.InternalServerError, ex.Message));
        }
    }
    
    [HttpPost("Remove")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<APIResult<bool>>> Remove([Required, EmailAddress] string email)
    {
        try
        {
            var result = await _waitlistService.RemoveFromWaitlist(email);
            
            return Ok(APIResult<bool>.Success(data: result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove email from waitlist");
            return BadRequest(APIResult<bool>.Error(HttpStatusCode.InternalServerError, ex.Message));
        }
    }
}