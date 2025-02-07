using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Repositories;

public class SubscriptionRepository(ILogger<ISubscriptionRepository> logger, DatabaseContext context) : ISubscriptionRepository
{
    public async Task<Subscription?> Get(string id)
    {
        return await context.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Subscription?> GetByStripeId(string stripeId)
    {
        return await context.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.StripeId == stripeId);
    }

    public async Task<Subscription?> GetByAccountId(string accountId)
    {
        return await context.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.AccountId == accountId);
    }

    public async Task<bool> Create(Subscription subscription)
    {
        if (await GetByAccountId(subscription.AccountId) != null) return false;
        
        await context.Subscriptions.AddAsync(subscription);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Update(Subscription subscription)
    {
        if (await GetByAccountId(subscription.AccountId) == null) return false;
        
        var local = context.Set<Subscription>().Local.FirstOrDefault(e => e.Id == subscription.Id);

        if (local != null)
        {
            context.Entry(local).State = EntityState.Detached;
        }

        context.Subscriptions.Update(subscription);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task Delete(string accountId)
    {
        var subscription = await GetByAccountId(accountId);
        if (subscription == null) return;
        
        context.Subscriptions.Remove(subscription);
        await context.SaveChangesAsync();
    }
}