using System.Collections.ObjectModel;
using AutoMapper;
using LudereAI.Core.Entities.Chat;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Shared.Enums;
using LudereAI.Shared.Models;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.Services;

public class ChatCoordinator : IChatCoordinator
{
    private readonly ILogger<ChatCoordinator> _logger;
    private readonly IChatService _chat;
    private readonly IAudioService _audio;
    private readonly IMapper _mapper;
    private readonly IChatSession _session;

    public ChatCoordinator(ILogger<ChatCoordinator> logger,
        IChatService chat,
        IAudioService audio,
        IMapper mapper,
        IChatSession session)
    {
        _logger = logger;
        _chat = chat;
        _audio = audio;
        _mapper = mapper;
        _session = session;
    }

    public async Task Initialize() => await RefreshConversations();

    public bool CanSend(string message, ConversationModel? conversation, string? gameContext)
        => !string.IsNullOrWhiteSpace(message)
           && !_session.IsAssistantThinking
           && conversation != null
           && !string.IsNullOrWhiteSpace(gameContext);

    public async Task RefreshConversations()
    {
        try
        {
            var conversations = await _chat.GetConversations();
            var mappedConversations = _mapper.Map<IEnumerable<ConversationModel>>(conversations).ToList();
            foreach (var m in mappedConversations.SelectMany(c => c.Messages))
                m.CreatedAt = m.CreatedAt.ToLocalTime();

            _session.SetConversations(new ObservableCollection<ConversationModel>(mappedConversations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh conversations");
        }
    }

    public async Task DeleteConversation(ConversationModel conversation)
    {
        try
        {
            var success = await _chat.DeleteConversation(conversation.Id);
            if (success)
            {
                _session.Conversations.Remove(conversation);
                if (_session.CurrentConversation == conversation)
                {
                    _session.CurrentConversation = _session.Conversations.FirstOrDefault();
                }

                await RefreshConversations();
            }
            else
            {
                _logger.LogWarning("Failed to delete conversation with ID {ConversationId}", conversation.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete conversation");
        }
    }

    public async Task SendMessage(string message, string? gameContext, WindowInfo? windowContext)
    {
        if (_session.CurrentConversation == null) return;

        try
        {
            _session.IsAssistantThinking = true;
            _session.AddMessageLocal(message, MessageRole.User);

            var result = await _chat.SendMessage(new ChatRequest
            {
                Message = message,
                ConversationId = _session.CurrentConversation.Id,
                GameContext = gameContext,
                Window = windowContext
            });

            if (result is { IsSuccessful: true, Value: not null })
            {
                UpdateConversationState(result.Value);
                var lastAssistantMessage =
                    _session.CurrentConversation.Messages.LastOrDefault(m => m.Role == MessageRole.Assistant);
                if (lastAssistantMessage?.Audio.Length > 0)
                {
                    await _audio.PlayAudioAsync(lastAssistantMessage.Audio);
                }
            }
            else
            {
                _session.AddSystemMessage(result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message");
            _session.AddSystemMessage("An error occurred while sending your message. Please try again.");
        }
        finally
        {
            _session.IsAssistantThinking = false;
        }
    }

    private void UpdateConversationState(Conversation conversation)
    {
        var updated = _mapper.Map<ConversationModel>(conversation);
        foreach (var m in updated.Messages) m.CreatedAt = m.CreatedAt.ToLocalTime();

        var existing = _session.Conversations.FirstOrDefault(c => c.Id == updated.Id);
        if (existing != null)
        {
            var index = _session.Conversations.IndexOf(existing);
            _session.Conversations[index] = updated;
        }
        else
        {
            _session.Conversations.Insert(0, updated);
        }

        _session.CurrentConversation = updated;
        _session.SetMessages(new ObservableCollection<MessageModel>(updated.Messages));
    }
}