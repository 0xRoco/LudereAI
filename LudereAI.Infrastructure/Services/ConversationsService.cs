using AutoMapper;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Services;

public class ConversationsService : IConversationsService
{
    private readonly ILogger<IConversationsService> _logger;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageService _messageService;
    private readonly IMapper _mapper;

    public ConversationsService(ILogger<IConversationsService> logger, IConversationRepository conversationRepository, IMapper mapper, IMessageService messageService)
    {
        _logger = logger;
        _conversationRepository = conversationRepository;
        _mapper = mapper;
        _messageService = messageService;
    }

    public async Task<IEnumerable<ConversationDTO>> GetConversations()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<ConversationDTO>> GetConversationsByUser(string userId)
    {
        var conversations = await _conversationRepository.GetByAccountId(userId);
        var dtos = _mapper.Map<IEnumerable<ConversationDTO>>(conversations);

        foreach (var dto in dtos)
        {
            var messages = await _messageService.GetMessages(dto.Id);
            dto.Messages = messages;
        }
        
        return _mapper.Map<IEnumerable<ConversationDTO>>(conversations);
    }

    public async Task<ConversationDTO?> GetConversation(string id)
    {
        var conversation = await _conversationRepository.Get(id);
        
        if (conversation != null)
        {
            conversation.Messages = conversation.Messages.OrderBy(x => x.CreatedAt);
        }
        
        return _mapper.Map<ConversationDTO>(conversation);
    }

    public async Task<bool> CreateConversation(ConversationDTO dto)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateConversation(ConversationDTO dto)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteConversation(string id)
    {
        throw new NotImplementedException();
    }
}