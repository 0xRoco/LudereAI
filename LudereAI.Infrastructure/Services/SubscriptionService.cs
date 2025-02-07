using AutoMapper;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Account;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Services;

public class SubscriptionService(ILogger<ISubscriptionService> logger,
    ISubscriptionRepository subscriptionRepository,
    IAccountRepository accountRepository,
    IMapper mapper) : ISubscriptionService 
{
    public async Task<SubscriptionDTO?> GetSubscription(string subscriptionId)
    {
        var subscription = await subscriptionRepository.Get(subscriptionId);
        var subscriptionDTO = mapper.Map<SubscriptionDTO>(subscription);
        
        return subscriptionDTO;
    }

    public async Task<SubscriptionDTO?> GetSubscriptionByAccountId(string accountId)
    {
        var subscription = await subscriptionRepository.GetByAccountId(accountId);
        var subscriptionDTO = mapper.Map<SubscriptionDTO>(subscription);
        
        return subscriptionDTO;
    }

    public async Task ActivateSubscription(string accountId, string subscriptionId, string priceId, DateTime startDate, DateTime endDate, string status = "")
    {
        try
        {
            var subStatus = string.IsNullOrWhiteSpace(status) ? SubscriptionStatus.Active : MapSubscriptionStatus(status);
            var existingSubscription = await subscriptionRepository.GetByAccountId(accountId);
            if (existingSubscription == null)
            {
                var subscription = new Subscription
                {
                    AccountId = accountId,
                    StripeId = subscriptionId,
                    SubscriptionPlan = MapPlanType(priceId),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    Status = subStatus,
                };
                await subscriptionRepository.Create(subscription);
            }
            else
            {
                existingSubscription.StripeId = subscriptionId;
                existingSubscription.SubscriptionPlan = MapPlanType(priceId);
                existingSubscription.StartDate = startDate;
                existingSubscription.EndDate = endDate;
                existingSubscription.Status = subStatus;

                await subscriptionRepository.Update(existingSubscription);
            }
            
            var account = await accountRepository.Get(accountId);
            if (account == null) return;
            account.Tier = existingSubscription?.SubscriptionPlan switch
            {
                SubscriptionPlan.Pro => SubscriptionTier.Pro,
                SubscriptionPlan.ProYearly => SubscriptionTier.Pro,
                SubscriptionPlan.Ultimate => SubscriptionTier.Ultimate,
                SubscriptionPlan.UltimateYearly => SubscriptionTier.Ultimate,
                _ => SubscriptionTier.Free
            };
            
            await accountRepository.Update(account);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error activating subscription for account {AccountId}", accountId);
            throw;
        }
        
    }

    public async Task CancelSubscription(string subscriptionId, DateTime cancelDate)
    {
        try
        {
            var subscription = await subscriptionRepository.GetByStripeId(subscriptionId);
            if (subscription == null) return;

            subscription.Status = SubscriptionStatus.Canceled;
            subscription.EndDate = cancelDate;

            var account = await accountRepository.Get(subscription.AccountId);
            if (account == null) return;
            
            account.Tier = SubscriptionTier.Free;
            await subscriptionRepository.Update(subscription);
            await accountRepository.Update(account);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error canceling subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task UpdateSubscription(string subscriptionId, string status, string priceId, DateTime currentPeriodEnd)
    {
        try
        {
            var subscription = await subscriptionRepository.GetByStripeId(subscriptionId);
            if (subscription == null) return;

            subscription.Status = MapSubscriptionStatus(status);
            subscription.SubscriptionPlan = MapPlanType(priceId);
            subscription.EndDate = currentPeriodEnd;
            
            var account = await accountRepository.Get(subscription.AccountId);
            if (account == null) return;

            account.Tier = subscription.SubscriptionPlan switch
            {
                SubscriptionPlan.Pro => SubscriptionTier.Pro,
                SubscriptionPlan.ProYearly => SubscriptionTier.Pro,
                SubscriptionPlan.Ultimate => SubscriptionTier.Ultimate,
                SubscriptionPlan.UltimateYearly => SubscriptionTier.Ultimate,
                _ => SubscriptionTier.Free
            };
            
            await subscriptionRepository.Update(subscription);
            await accountRepository.Update(account);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task RenewSubscription(string subscriptionId, DateTime newEndDate,string priceId, string status = "")
    {
        try
        {
            var subscription = await subscriptionRepository.GetByStripeId(subscriptionId);
            if (subscription == null) return;
        
            subscription.EndDate = newEndDate;
            subscription.Status = string.IsNullOrWhiteSpace(status) ? SubscriptionStatus.Active : MapSubscriptionStatus(status);
            subscription.SubscriptionPlan = MapPlanType(priceId);
        
            var account = await accountRepository.Get(subscription.AccountId);
            if (account == null) return;
            
            account.Tier = subscription.SubscriptionPlan switch
            {
                SubscriptionPlan.Pro => SubscriptionTier.Pro,
                SubscriptionPlan.ProYearly => SubscriptionTier.Pro,
                SubscriptionPlan.Ultimate => SubscriptionTier.Ultimate,
                SubscriptionPlan.UltimateYearly => SubscriptionTier.Ultimate,
                _ => SubscriptionTier.Free
            };
            
            await subscriptionRepository.Update(subscription);
            await accountRepository.Update(account);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error renewing subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task HandleFailedPayment(string subscriptionId, DateTime failureDate)
    {
        try
        {
            var subscription = await subscriptionRepository.GetByStripeId(subscriptionId);
            if (subscription == null) return;
        
            subscription.Status = SubscriptionStatus.PastDue;
            subscription.EndDate = failureDate;
        
            await subscriptionRepository.Update(subscription);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling failed payment for subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }


    private static SubscriptionPlan MapPlanType(string plan)
    {
        if (string.IsNullOrWhiteSpace(plan)) throw new ArgumentNullException(nameof(plan));
        
        return plan switch
        {
            "price_1QnSNQIC8mU8ubEvfggFA6l7" => SubscriptionPlan.Pro,
            "price_1QnSPRIC8mU8ubEvQmB2U0BK" => SubscriptionPlan.ProYearly,
            "price_1QnSNnIC8mU8ubEvjvSxX0dV" => SubscriptionPlan.Ultimate,
            "price_1QnSQ8IC8mU8ubEv3nwoEmeq" => SubscriptionPlan.UltimateYearly,
            _ => throw new ArgumentException($"Invalid plan type: {plan}", nameof(plan))
        };
    }
    
    private static SubscriptionStatus MapSubscriptionStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status)) throw new ArgumentNullException(nameof(status));
        
        return status switch
        {
            "active" => SubscriptionStatus.Active,
            "canceled" => SubscriptionStatus.Canceled,
            "incomplete" => SubscriptionStatus.Unknown,
            "incomplete_expired" => SubscriptionStatus.Unknown,
            "past_due" => SubscriptionStatus.PastDue,
            "trialing" => SubscriptionStatus.Unknown,
            "unpaid" => SubscriptionStatus.Unknown,
            _ => SubscriptionStatus.Unknown
        };
    }
    
}