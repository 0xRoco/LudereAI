using LudereAI.Application.Interfaces;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Account;
using LudereAI.Domain.Models.Chat;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Repositories;

public class AccountRepository(ILogger<IAccountRepository> logger,
    DatabaseContext context,
    ISubscriptionRepository subscriptionRepository,
    IConversationRepository conversationRepository) : IAccountRepository
{
    public async Task<IEnumerable<Account>> GetAllAsync()
    {
        return await context.Accounts.AsNoTracking().ToListAsync();
    }

    public async Task<Account?> GetAsync(string accountId)
    {
        var account =  await context.Accounts.FindAsync(accountId);
        if (account == null) return null;

        var (conversations, subscription) = await GetAccountData(accountId);
        account.Conversations = conversations.ToList();
        account.Subscription = subscription;
        
        return account;
    }

    public async Task<Account?> GetByUsernameAsync(string username)
    {
        var account = await context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Username == username);
        if (account == null) return null;
        
        var (conversations, subscription) = await GetAccountData(account.Id);
        account.Conversations = conversations.ToList();
        account.Subscription = subscription;
        
        return account;
    }

    public async Task<Account?> GetByEmailAsync(string email)
    {
        var account = await context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Email == email);
        if (account == null) return null;
        
        var (conversations, subscription) = await GetAccountData(account.Id);
        account.Conversations = conversations.ToList();
        account.Subscription = subscription;
        
        return account;
    }

    public async Task<bool> CreateAsync(Account account)
    {
        try
        {
            if (await GetByUsernameAsync(account.Username) != null || await GetAsync(account.Id) != null) return false;
            
            await context.Accounts.AddAsync(account);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating account");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Account account)
    {
        try
        {
            if (await GetAsync(account.Id) == null) return false;
            
            var trackedEntity = context.ChangeTracker
                .Entries<Account>().FirstOrDefault(e=> e.Entity.Id == account.Id);
            
            if (trackedEntity != null) trackedEntity.State = EntityState.Detached;
            
            account.UpdatedAt = DateTime.UtcNow;
            
            context.Accounts.Attach(account);
            context.Entry(account).State = EntityState.Modified;
            
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating account");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string accountId)
    {
        var account = await GetAsync(accountId);
        if (account == null) return false;
        
        account.DeletedAt = DateTime.UtcNow;

        return await UpdateAsync(account);
    }
    
    private async Task<(IEnumerable<Conversation> conversations, Subscription? subscription)> GetAccountData(string accountId)
    {
        var conversations = await conversationRepository.GetConversationsByAccountId(accountId);
        var subscription = await subscriptionRepository.GetByAccountId(accountId);

        return (conversations, subscription);
    }
}