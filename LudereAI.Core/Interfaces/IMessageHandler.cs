using LudereAI.Core.Entities;
using LudereAI.Core.Entities.Chat;
using LudereAI.Shared.DTOs;
using OpenAI.Chat;

namespace LudereAI.Core.Interfaces;

public interface IMessageHandler
{
    Task<List<ChatMessage>> BuildMessageHistory(AssistantRequest request, Conversation conversation);
    Task SaveConversationMessages(AssistantRequest request, AIResponse response);
}