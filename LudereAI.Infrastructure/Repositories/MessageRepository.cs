using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Chat;
using Microsoft.EntityFrameworkCore;

namespace LudereAI.Infrastructure.Repositories;

public class MessageRepository(DatabaseContext context) : IMessageRepository
{
    public async Task<IEnumerable<Message>> GetAllAsync()
    {
        return await context.Messages.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetMessagesAsync(string conversationId)
    {
        return await context.Messages.AsNoTracking().Where(m => m.ConversationId == conversationId).ToListAsync();
    }

    public async Task<bool> CreateAsync(Message message)
    {
        var conv = await context.Conversations.FirstOrDefaultAsync(c => c.Id == message.ConversationId);
        if (conv == null) return false;
        
        await context.Messages.AddAsync(message);
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> UpdateAsync(Message message)
    {
        var msg = await context.Messages.FirstOrDefaultAsync(m => m.Id == message.Id);
        if (msg == null) return false;
        
        msg.Content = message.Content;
        msg.Role = message.Role;
        
        msg.UpdatedAt = DateTime.UtcNow;
        
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteAsync(string messageId)
    {
        var msg = await context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
        if (msg == null) return false;
        
        msg.DeletedAt = DateTime.UtcNow;
        msg.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        
        return true;
    }
}