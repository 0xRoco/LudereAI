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
public class AssistantController(ILogger<AssistantController> logger,
    IOpenAIService openAIService, 
    IConversationRepository conversationRepository, 
    IAccountRepository accountRepository) : ControllerBase
{
    [RequireFeature("Assistant.Enabled")]
    [HttpPost]
    public async Task<ActionResult<APIResult<MessageDTO>>> SendMessage([FromBody] AssistantRequestDTO requestDto)
    {
        try
        {
            if (!requestDto.IsMessageValid())
            {
                logger.LogWarning("Empty message received in CreateMessage");
                return BadRequest(APIResult<MessageDTO>.Error(HttpStatusCode.BadRequest, "Message cannot be empty"));
            }

            var accountId = await ValidateAndGetAccountId();
            if (accountId.Result != null) return accountId.Result;

            var conversation = await GetOrCreateConversation(requestDto.ConversationId, accountId.AccountId!);
            if (conversation.Result != null) return conversation.Result;

            var response = await openAIService.SendMessageAsync(conversation.Conversation!, requestDto);
            logger.LogInformation("Successfully processed message for conversation {ConversationId}", conversation.Conversation!.Id);

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
            logger.LogError(ex, "Error processing message in CreateMessage");
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

        var account = await accountRepository.GetAsync(accountId);
        if (account == null)
        {
            return (Unauthorized(APIResult<MessageDTO>.Error(HttpStatusCode.Unauthorized, "Invalid user")), null);
        }
        
        return (null, accountId);
    }

    private async Task<(ActionResult? Result, Domain.Models.Chat.Conversation? Conversation)> GetOrCreateConversation(string conversationId, string accountId)
    {
        var conversation = await conversationRepository.GetConversationAsync(conversationId);
        
        logger.LogDebug("Conversation: {conversation}", conversation?.ToJson());
    
        if (conversation == null)
        {
            conversation = new Conversation
            {
                AccountId = accountId
            };
            await conversationRepository.CreateConversationAsync(conversation);
            return (null, conversation);
        }

        if (conversation.AccountId != accountId)
        {
            return (Unauthorized(APIResult<MessageDTO>.Error(HttpStatusCode.Unauthorized, "Invalid user")), null);
        }

        return (null, conversation);
    }
}