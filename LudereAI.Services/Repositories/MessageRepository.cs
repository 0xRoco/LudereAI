using LudereAI.Core.Entities.Chat;
using LudereAI.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LudereAI.Services.Repositories;

public class MessageRepository(DatabaseContext context) : IMessageRepository
{
    public async Task<IEnumerable<Message>> GetAll()
    {
        return await context.Messages
            .AsNoTracking()
            .OrderBy(m => m.CreatedAt)
            .Include(m => m.Screenshot)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetMessages(string conversationId)
    {
        return await context.Messages.AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Include(message => message.Screenshot)
            .ToListAsync();
    }

    public async Task<bool> Create(Message message)
    {
        var conv = await context.Conversations.FirstOrDefaultAsync(c => c.Id == message.ConversationId);
        if (conv == null) return false;
        
        await context.Messages.AddAsync(message);
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> CreateBatch(IEnumerable<Message> messages)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            foreach (var message in messages)
            {
                var conv = await context.Conversations.FirstOrDefaultAsync(c => c.Id == message.ConversationId);
                if (conv != null) continue;
                await transaction.RollbackAsync();
                return false;
            }

            await context.Messages.AddRangeAsync(messages);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> Update(Message message)
    {
        var msg = await context.Messages.FirstOrDefaultAsync(m => m.Id == message.Id);
        if (msg == null) return false;
        
        context.Messages.Entry(message).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(string messageId)
    {
        var msg = await context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
        if (msg == null) return false;
        
        context.Messages.Entry(msg).State = EntityState.Deleted;
        await context.SaveChangesAsync();
        return true;
    }
}