using AutoMapper;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly ILogger<IMessageService> _logger;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessageService(ILogger<IMessageService> logger, IMessageRepository messageRepository, IMapper mapper)
    {
        _logger = logger;
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MessageDTO>> GetMessages(string conversationId)
    {
        var messages = await _messageRepository.GetMessagesAsync(conversationId);
        var dtos = _mapper.Map<IEnumerable<MessageDTO>>(messages);
        return dtos;
    }

    public async Task<MessageDTO?> GetMessage(string conversationId, string messageId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CreateMessage(string conversationId, MessageDTO dto)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateMessage(string conversationId, MessageDTO dto)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteMessage(string conversationId, string messageId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteMessages(string conversationId)
    {
        throw new NotImplementedException();
    }
}