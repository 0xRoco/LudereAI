using LudereAI.Core.Entities.Chat;
using LudereAI.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LudereAI.Services.Repositories;

public class MessageRepository(ILogger<IMessageRepository> _logger, DatabaseContext context) : IMessageRepository
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
        var messageList = messages.ToList();
        if (messageList.Count == 0) return true;
        
        var  conversationId = messageList.First().ConversationId;
        
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var conv = await context.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId);
            if (conv == null)
            {
                _logger.LogWarning($"Attempted to add messages to a non-existent conversation: {conversationId}");
                await transaction.RollbackAsync();
                return false;
            }

            foreach (var message in messageList)
            {
                message.Conversation = conv;
                message.ConversationId = conv.Id;
            }

            await context.Messages.AddRangeAsync(messageList);
            context.Conversations.Entry(conv).State = EntityState.Modified;
            
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating batch messages for conversation {conversationId}");
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