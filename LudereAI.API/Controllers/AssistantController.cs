using System.Net;
using System.Security.Claims;
using LudereAI.API.Core;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Chat;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.API.Controllers;

[ApiController, Authorize, Route("api/[controller]")]
public class AssistantController : ControllerBase
{
    private readonly ILogger<AssistantController> _logger;
    private readonly IOpenAIService _openAIService;
    private readonly IConversationRepository _conversationRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountUsageService _accountUsageService;

    public AssistantController(ILogger<AssistantController> logger,
        IOpenAIService openAIService, 
        IConversationRepository conversationRepository, 
        IAccountRepository accountRepository, IAccountUsageService accountUsageService)
    {
        _logger = logger;
        _openAIService = openAIService;
        _conversationRepository = conversationRepository;
        _accountRepository = accountRepository;
        _accountUsageService = accountUsageService;
    }

    [RequireFeature("Assistant.Enabled")]
    [HttpPost]
    public async Task<ActionResult<APIResult<MessageDTO>>> SendMessage([FromBody] AssistantRequestDTO requestDto)
    {
        try
        {
            if (!requestDto.IsMessageValid())
            {
                _logger.LogWarning("Empty message received in CreateMessage");
                return BadRequest(APIResult<MessageDTO>.Error(HttpStatusCode.BadRequest, "Message cannot be empty"));
            }

            var validationResult = await ValidateAndGetAccountId();
            if (validationResult.Result != null) return validationResult.Result;
            
            if (!await _accountUsageService.CanSendMessage(validationResult.AccountId!))
            {
                return BadRequest(APIResult<MessageDTO>.Error(HttpStatusCode.BadRequest, "Daily message limit reached", new MessageDTO
                {
                    Content = "Daily message limit reached",
                    Role = MessageRole.System,
                }));
            }

            if (!string.IsNullOrWhiteSpace(requestDto.Screenshot))
            {
                if (!await _accountUsageService.CanAnalyseScreenshot(validationResult.AccountId!))
                {
                    return BadRequest(APIResult<MessageDTO>.Error(HttpStatusCode.BadRequest, "Daily screenshot analysis limit reached", new MessageDTO()
                    {
                        Content = "Daily screenshot analysis limit reached",
                        Role = MessageRole.System
                    }));
                }
            }

            var conversation = await GetOrCreateConversation(requestDto.ConversationId, validationResult.AccountId!);
            if (conversation.Result != null) return conversation.Result;

            var response = await _openAIService.SendMessageAsync(conversation.Conversation!, requestDto);
            await _accountUsageService.IncrementMessageCount(validationResult.AccountId!);
            if (!string.IsNullOrWhiteSpace(requestDto.Screenshot))
            {
                await _accountUsageService.IncrementScreenshotCount(validationResult.AccountId!);
            }
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

    private async Task<(ActionResult? Result, Domain.Models.Chat.Conversation? Conversation)> GetOrCreateConversation(string conversationId, string accountId)
    {
        var conversation = await _conversationRepository.GetConversationAsync(conversationId);
        
        _logger.LogDebug("Conversation: {conversation}", conversation?.ToJson());
    
        if (conversation == null)
        {
            conversation = new Conversation
            {
                AccountId = accountId
            };
            await _conversationRepository.CreateConversationAsync(conversation);
            return (null, conversation);
        }

        if (conversation.AccountId != accountId)
        {
            return (Unauthorized(APIResult<MessageDTO>.Error(HttpStatusCode.Unauthorized, "Invalid user")), null);
        }

        return (null, conversation);
    }
}