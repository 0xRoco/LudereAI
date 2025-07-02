using LudereAI.Core.Entities.Chat;
using LudereAI.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LudereAI.Services.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly ILogger<IConversationRepository> _logger;
    private readonly DatabaseContext _context;
    private readonly IMessageRepository _messageRepository;

    public ConversationRepository(ILogger<IConversationRepository> logger, DatabaseContext context, IMessageRepository messageRepository)
    {
        _logger = logger;
        _context = context;
        _messageRepository = messageRepository;
    }

    public async Task<Conversation?> Get(string conversationId)
    {
        var conv = await _context.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId);
        if (conv == null) return null;
        
        conv.Messages = await _messageRepository.GetMessages(conversationId);
        return conv;
    }

    public async Task<IEnumerable<Conversation>> GetAll()
    {
        return await _context.Conversations
            .AsNoTracking()
            .OrderByDescending(c=>c.UpdatedAt)
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .ToListAsync();
        
    }

    public async Task<bool> Create(Conversation conversation)
    {
        try
        {
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create conversation with ID {ConversationId}", conversation.Id);
            return false;
        }
    }

    public async Task<bool> Update(Conversation conversation)
    {
        try
        {
            _context.Conversations.Update(conversation);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update conversation with ID {ConversationId}", conversation.Id);
            return false;
        }
    }

    public async Task<bool> Delete(string conversationId)
    {
        try
        {
            var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId);
            if (conversation == null) return false;
            
            _context.Conversations.Entry(conversation).State = EntityState.Deleted;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete conversation with ID {ConversationId}", conversationId);
            return false;
        }
    }
}