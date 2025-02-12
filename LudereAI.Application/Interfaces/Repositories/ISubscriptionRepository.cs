using LudereAI.Domain.Models.Account;

namespace LudereAI.Application.Interfaces.Repositories;

public interface ISubscriptionRepository
{
    Task<UserSubscription?> Get(string id);
    Task<UserSubscription?> GetByStripeId(string stripeId);
    Task<UserSubscription?> GetByAccountId(string accountId);
    Task<bool> Create(UserSubscription userSubscription);
    Task<bool> Update(UserSubscription userSubscription);
    Task Delete(string accountId);
}