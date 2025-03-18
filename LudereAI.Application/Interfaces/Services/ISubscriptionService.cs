using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Stripe;

namespace LudereAI.Application.Interfaces.Services;

public interface ISubscriptionService
{
    Task<UserSubscriptionDTO?> GetSubscription(string subscriptionId);
    Task<UserSubscriptionDTO?> GetSubscriptionByAccountId(string accountId);
    
    Task CreateSubscription(Subscription subscription);
    Task UpdateSubscription(Subscription subscription);
    Task HandleSubscriptionCanceled(Subscription subscription);
    Task HandlePaymentFailure(Invoice invoice);
    Task HandleTrialEnding(Subscription subscription);
    
    Task<bool> HasActiveSubscription(string accountId);
    Task<bool> IsTrialing(string accountId);
    
    Task SyncSubscription(string subscriptionId);
    
}