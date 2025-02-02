using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;

namespace LudereAI.Application.Interfaces.Services;

public interface ISubscriptionService
{
    Task<SubscriptionDTO?> GetSubscription(string subscriptionId);
    Task<SubscriptionDTO?> GetSubscriptionByAccountId(string accountId);
    Task ActivateSubscription(string accountId, string subscriptionId, string priceId, DateTime startDate, DateTime endDate, string status = "");
    Task CancelSubscription(string subscriptionId, DateTime cancelDate);
    Task UpdateSubscription(string subscriptionId, string status, string priceId, DateTime currentPeriodEnd);
    Task RenewSubscription(string subscriptionId, DateTime newEndDate,string priceId, string status = "");
    Task HandleFailedPayment(string subscriptionId, DateTime failureDate);
}