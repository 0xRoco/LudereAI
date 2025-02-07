using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Domain.Models.Account;
using LudereAI.Domain.Models.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Repositories;

public class AccountRepository(ILogger<IAccountRepository> logger,
    DatabaseContext context,
    ISubscriptionRepository subscriptionRepository,
    IConversationRepository conversationRepository) : IAccountRepository
{
    public async Task<IEnumerable<Account>> GetAll()
    {
        return await context.Accounts.AsNoTracking().ToListAsync();
    }

    public async Task<Account?> Get(string accountId)
    {
        var account =  await context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == accountId);
        if (account == null) return null;

        var (conversations, subscription) = await GetAccountData(accountId);
        account.Conversations = conversations.ToList();
        account.Subscription = subscription;
        
        return account;
    }

    public async Task<Account?> GetByUsername(string username)
    {
        var account = await context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Username == username);
        if (account == null) return null;
        
        var (conversations, subscription) = await GetAccountData(account.Id);
        account.Conversations = conversations.ToList();
        account.Subscription = subscription;
        
        return account;
    }

    public async Task<Account?> GetByEmail(string email)
    {
        var account = await context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Email == email);
        if (account == null) return null;
        
        var (conversations, subscription) = await GetAccountData(account.Id);
        account.Conversations = conversations.ToList();
        account.Subscription = subscription;
        
        return account;
    }

    public async Task<bool> Create(Account account)
    {
        try
        {
            if (await GetByUsername(account.Username) != null || await Get(account.Id) != null) return false;
            
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

    public async Task<bool> Update(Account account)
    {
        try
        {
            if (await Get(account.Id) == null) return false;
            
            var local = context.Set<Account>()
                .Local
                .FirstOrDefault(entry => entry.Id.Equals(account.Id));
            
            if (local != null)
            {
                context.Entry(local).State = EntityState.Detached;
            }
            
            context.Accounts.Update(account);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating account");
            return false;
        }
    }

    public async Task<bool> Delete(string accountId)
    {
        var account = await Get(accountId);
        if (account == null) return false;

        context.Accounts.Remove(account);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateLastLogin(string accountId)
    {
        var account = await Get(accountId);
        if (account == null) return false;

        account.LastLogin = DateTime.UtcNow;
        return await Update(account);
    }

    private async Task<(IEnumerable<Conversation> conversations, Subscription? subscription)> GetAccountData(string accountId)
    {
        var conversations = await conversationRepository.GetConversationsByAccountId(accountId);
        var subscription = await subscriptionRepository.GetByAccountId(accountId);

        return (conversations, subscription);
    }
}