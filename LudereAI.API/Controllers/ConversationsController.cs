using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.API.Controllers;


[ApiController, Route("[controller]")]
public class ConversationsController : ControllerBase
{ 
    private readonly ILogger<ConversationsController> _logger;
    private readonly IConversationsService _conversationsService;

    public ConversationsController(ILogger<ConversationsController> logger, IConversationsService conversationsService)
    {
        _logger = logger;
        _conversationsService = conversationsService;
    }
    
    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<ConversationDTO>>> GetConversations()
    {
        try
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(accountId))
            {
                _logger.LogWarning("Account id not found in claims");
                return BadRequest(APIResult<IEnumerable<ConversationDTO>>.Error(HttpStatusCode.BadRequest, "Account id not found in claims"));
            }
            
            var conversations = await _conversationsService.GetConversationsByUser(accountId);
            
            return Ok(APIResult<IEnumerable<ConversationDTO>>.Success(data: conversations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                APIResult<IEnumerable<ConversationDTO>>.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred"));
        }
    }
    
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ConversationDTO>> GetConversation([Required] string id)
    {
        try
        {
            var conversation = await _conversationsService.GetConversation(id);
            
            if (conversation != null) return Ok(APIResult<ConversationDTO>.Success(data: conversation));
            
            _logger.LogWarning("Conversation not found for id {ConversationId}", id);
            return NotFound(APIResult<ConversationDTO>.Error(HttpStatusCode.NotFound, "Conversation not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation for id {ConversationId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                APIResult<ConversationDTO>.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred"));
        }
    }
}