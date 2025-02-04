using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Domain.Models.Chat;
using Microsoft.EntityFrameworkCore;

namespace LudereAI.Infrastructure.Repositories;

public class ConversationRepository(DatabaseContext context, IMessageRepository messageRepository) : IConversationRepository
{
    public async Task<Conversation?> GetConversationAsync(string conversationId)
    {
        var conv =  await context.Conversations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == conversationId);
        if (conv == null) return null;
        
        conv.Messages = await messageRepository.GetMessagesAsync(conversationId);
        return conv;
    }

    public async Task<IEnumerable<Conversation>> GetConversationsByAccountId(string accountId)
    {
        var conv = await context.Conversations.AsNoTracking().Where(c => c.AccountId == accountId)
            .Include(conversation => conversation.Messages).ToListAsync();

        conv = conv.OrderByDescending(c => c.CreatedAt).ToList();
        
        foreach (var conversation in conv)
        {
            conversation.Messages = conversation.Messages.OrderBy(m => m.CreatedAt).ToList();
        }
        
        return conv;
    }

    public async Task<bool> CreateConversationAsync(Conversation conversation)
    {
        try
        {
            await context.Conversations.AddAsync(conversation);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}