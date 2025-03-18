using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Chat;
using LudereAI.Shared.DTOs;
using OpenAI.Chat;

namespace LudereAI.Application.Interfaces;

public interface IMessageHandler
{
    Task<StoredFile?> HandleScreenshot(AssistantRequestDTO request, Conversation conversation);
    Task<List<ChatMessage>> BuildMessageHistory(AssistantRequestDTO request, Conversation conversation);

    Task SaveConversationMessages(AssistantRequestDTO request, AIResponse response,
        string screenshotId = "");
}