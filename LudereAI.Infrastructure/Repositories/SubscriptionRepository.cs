using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Domain.Models.Account;
using LudereAI.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Repositories;

public class SubscriptionRepository(ILogger<ISubscriptionRepository> logger, DatabaseContext context) : ISubscriptionRepository
{
    public async Task<UserSubscription?> Get(string id)
    {
        return await context.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<UserSubscription?> GetByStripeId(string stripeId)
    {
        return await context.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeId);
    }

    public async Task<UserSubscription?> GetByAccountId(string accountId)
    {
        return await context.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.AccountId == accountId);
    }

    public async Task<bool> Create(UserSubscription userSubscription)
    {
        if (await GetByAccountId(userSubscription.AccountId) != null) return false;
        
        await context.Subscriptions.AddAsync(userSubscription);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Update(UserSubscription userSubscription)
    {
        if (await GetByAccountId(userSubscription.AccountId) == null) return false;


        context.Subscriptions.Entry(userSubscription).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task Delete(string accountId)
    {
        var subscription = await GetByAccountId(accountId);
        if (subscription == null) return;
        
        context.Subscriptions.Entry(subscription).State = EntityState.Deleted;
        await context.SaveChangesAsync();
    }
}