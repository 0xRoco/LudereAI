using System.Net;
using System.Security.Claims;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models.Chat;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.API.Controllers;

[ApiController, Authorize, Route("[controller]")]
public class AssistantController : ControllerBase
{
    private readonly ILogger<AssistantController> _logger;
    private readonly IOpenAIService _openAIService;
    private readonly IConversationRepository _conversationRepository;
    private readonly IAccountRepository _accountRepository;

    public AssistantController(ILogger<AssistantController> logger,
        IOpenAIService openAIService, 
        IConversationRepository conversationRepository, 
        IAccountRepository accountRepository)
    {
        _logger = logger;
        _openAIService = openAIService;
        _conversationRepository = conversationRepository;
        _accountRepository = accountRepository;
    }

    [HttpPost]
    public async Task<ActionResult<APIResult<MessageDTO>>> SendMessage([FromBody] AssistantRequestDTO requestDto)
    {
        try
        {
            if (!requestDto.IsMessageValid())
            {
                return BadRequest(APIResult<MessageDTO>.Error(HttpStatusCode.BadRequest, "Message cannot be empty"));
            }

            var validationResult = await ValidateAndGetAccountId();
            if (validationResult.Result != null || string.IsNullOrWhiteSpace(validationResult.AccountId))
                return validationResult.Result
                       ?? Unauthorized(APIResult<MessageDTO>.Error(HttpStatusCode.Unauthorized, "Invalid user"));

            var conversation = await GetOrCreateConversation(requestDto.ConversationId, validationResult.AccountId, requestDto.GameContext);
            if (conversation.Result != null || conversation.Conversation == null) return conversation.Result ?? BadRequest(APIResult<MessageDTO>.Error(HttpStatusCode.BadRequest, "Invalid conversation"));

            var response = await _openAIService.SendMessageAsync(conversation.Conversation, requestDto);
            
            _logger.LogInformation("Successfully processed message for conversation {ConversationId}", conversation.Conversation!.Id);

            var message = new MessageDTO
            {
                Id = response.MessageId,
                ConversationId = response.ConversationId,
                Content = response.Message,
                Audio = response.ttsAudio,
                Role = MessageRole.Assistant
            };

            return Ok(APIResult<MessageDTO>.Success(data: message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message in CreateMessage");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                APIResult<MessageDTO>.Error(HttpStatusCode.InternalServerError, "An unexpected error occurred"));
        }
    }

    [HttpPost("PredictGame")]
    public async Task<ActionResult<APIResult<ProcessInfoDTO>>> PredictGame([FromBody] List<ProcessInfoDTO> dtos)
    {
        if (dtos.Count == 0)
        {
            return BadRequest(APIResult<ProcessInfoDTO>.Error(HttpStatusCode.BadRequest, "No processes provided"));
        }
        
        var validationResult = await ValidateAndGetAccountId();
        if (validationResult.Result != null || string.IsNullOrWhiteSpace(validationResult.AccountId))
            return validationResult.Result
                   ?? Unauthorized(APIResult<MessageDTO>.Error(HttpStatusCode.Unauthorized, "Invalid user"));
        
        var response = await _openAIService.PredictGame(dtos);
        return Ok(APIResult<ProcessInfoDTO>.Success(data: response));
    }
    
    
    private async Task<(ActionResult? Result, string? AccountId)> ValidateAndGetAccountId()
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null)
        {
            return (Unauthorized(APIResult<MessageDTO>.Error(HttpStatusCode.Unauthorized, "Invalid user")), null);
        }

        var account = await _accountRepository.Get(accountId);
        if (account == null)
        {
            return (Unauthorized(APIResult<MessageDTO>.Error(HttpStatusCode.Unauthorized, "Invalid user")), null);
        }
        
        return (null, accountId);
    }

    private async Task<(ActionResult? Result, Conversation? Conversation)> GetOrCreateConversation(string conversationId, string accountId, string gameContext)
    {
        var conversation = await _conversationRepository.Get(conversationId);
        
        if (conversation == null)
        {
            conversation = new Conversation
            {
                AccountId = accountId,
                GameContext = gameContext
            };
            await _conversationRepository.Create(conversation);
            return (null, conversation);
        }

        if (conversation.AccountId != accountId)
        {
            return (Unauthorized(APIResult<MessageDTO>.Error(HttpStatusCode.Unauthorized, "Invalid user")), null);
        }

        if (conversation.GameContext != gameContext)
        {
            return (BadRequest(APIResult<MessageDTO>.Error(HttpStatusCode.BadRequest, 
                "Active conversation game context does not match the request")), null);
        }

        return (null, conversation);
    }
}