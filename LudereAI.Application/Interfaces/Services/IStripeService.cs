using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Account;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Stripe.Checkout;

namespace LudereAI.Application.Interfaces.Services;

public interface IStripeService
{
    Task<Session?> CreateCheckoutSession(AccountDTO dto, string priceId, SubscriptionPlan subscriptionPlan);
    Task SyncAllAccounts();
    Task<string> CreateOrSyncAccount(Account account);
}