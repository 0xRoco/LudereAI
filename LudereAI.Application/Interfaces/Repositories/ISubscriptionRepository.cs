using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Account;

namespace LudereAI.Application.Interfaces.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> Get(string id);
    Task<Subscription?> GetByStripeId(string stripeId);
    Task<Subscription?> GetByAccountId(string accountId);
    Task<bool> Create(Subscription subscription);
    Task<bool> Update(Subscription subscription);
    Task Delete(string accountId);
}