using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Domain.Models.Chat;
using Microsoft.EntityFrameworkCore;

namespace LudereAI.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly DatabaseContext _context;
    private readonly IMessageRepository _messageRepository;

    public ConversationRepository(DatabaseContext context, IMessageRepository messageRepository)
    {
        _context = context;
        _messageRepository = messageRepository;
    }

    public async Task<Conversation?> Get(string conversationId)
    {
        var conv =  await _context.Conversations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == conversationId);
        if (conv == null) return null;
        
        conv.Messages = await _messageRepository.GetMessagesAsync(conversationId);
        return conv;
    }

    public async Task<IEnumerable<Conversation>> GetByAccountId(string accountId)
    {
        var conv = await _context.Conversations.AsNoTracking().Where(c => c.AccountId == accountId)
            .Include(conversation => conversation.Messages).ToListAsync();

        conv = conv.OrderByDescending(c => c.UpdatedAt).ToList();
        
        foreach (var conversation in conv)
        {
            conversation.Messages = conversation.Messages.OrderBy(m => m.CreatedAt).ToList();
        }
        
        return conv;
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
            return false;
        }
    }

    public async Task<bool> Update(Conversation conversation)
    {
        try
        {
            _context.Conversations.Entry(conversation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
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
            return false;
        }
    }
}