using AutoMapper;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models.Account;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.Extensions.Logging;
using Stripe;
using Account = LudereAI.Domain.Models.Account.Account;

namespace LudereAI.Infrastructure.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ILogger<ISubscriptionService> _logger;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly Stripe.SubscriptionService _stripeSubscriptionService;
        private readonly IMapper _mapper;

        public SubscriptionService(
            ILogger<ISubscriptionService> logger,
            ISubscriptionRepository subscriptionRepository,
            IAccountRepository accountRepository,
            Stripe.SubscriptionService stripeSubscriptionService,
            IMapper mapper)
        {
            _logger = logger;
            _subscriptionRepository = subscriptionRepository;
            _accountRepository = accountRepository;
            _stripeSubscriptionService = stripeSubscriptionService;
            _mapper = mapper;
        }

        public async Task<UserSubscriptionDTO?> GetSubscription(string subscriptionId)
        {
            var subscription = await _subscriptionRepository.GetByStripeId(subscriptionId);
            return _mapper.Map<UserSubscriptionDTO>(subscription);
        }

        public async Task<UserSubscriptionDTO?> GetSubscriptionByAccountId(string accountId)
        {
            var subscription = await _subscriptionRepository.GetByAccountId(accountId);
            return _mapper.Map<UserSubscriptionDTO>(subscription);
        }

        public async Task CreateSubscription(Subscription stripeSubscription)
        {
            if (!stripeSubscription.Metadata.TryGetValue("AccountId", out var accountId) ||
                string.IsNullOrWhiteSpace(accountId))
            {
                throw new ArgumentException($"Stripe subscription {stripeSubscription.Id} is missing AccountId metadata.");
            }

            var account = await _accountRepository.Get(accountId) 
                ?? throw new ArgumentException($"Account with ID {accountId} not found.");

            var existingSub = await _subscriptionRepository.GetByAccountId(accountId);
            if (existingSub != null)
            {
                await UpdateExistingSubscription(existingSub, stripeSubscription, account);
                return;
            }

            var newSub = new UserSubscription
            {
                AccountId = accountId,
                StripeCustomerId = stripeSubscription.CustomerId,
                StripeSubscriptionId = stripeSubscription.Id,
                Status = MapSubscriptionStatus(stripeSubscription.Status),
                Plan = MapPlanType(stripeSubscription.Items.Data[0].Price.Id ?? ""),
                CurrentPeriodStart = stripeSubscription.CurrentPeriodStart,
                CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd,
                CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd,
            };

            await _subscriptionRepository.Create(newSub);
            account.Tier = MapPlanToTier(newSub.Plan);
            await _accountRepository.Update(account);
        }

        public async Task UpdateSubscription(Subscription stripeSubscription)
        {
            var existingSub = await _subscriptionRepository.GetByStripeId(stripeSubscription.Id);
            if (existingSub == null)
            {
                await CreateSubscription(stripeSubscription);
                return;
            }

            var account = await _accountRepository.Get(existingSub.AccountId) 
                ?? throw new ArgumentException($"Account not found for subscription {stripeSubscription.Id}");

            await UpdateExistingSubscription(existingSub, stripeSubscription, account);
        }

        private async Task UpdateExistingSubscription(UserSubscription existingSub, Subscription stripeSubscription,
            Account account)
        {
            existingSub.StripeCustomerId = stripeSubscription.CustomerId;
            existingSub.StripeSubscriptionId = stripeSubscription.Id;
            existingSub.Status = MapSubscriptionStatus(stripeSubscription.Status);
            existingSub.Plan = MapPlanType(stripeSubscription.Items.Data[0].Price.Id ?? "");
            existingSub.CurrentPeriodStart = stripeSubscription.CurrentPeriodStart;
            existingSub.CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd;
            existingSub.CancelAtPeriodEnd = stripeSubscription.CancelAtPeriodEnd;

            account.Tier = MapPlanToTier(existingSub.Plan);

            await _subscriptionRepository.Update(existingSub);
            await _accountRepository.Update(account);
        }

        public async Task HandleSubscriptionCanceled(Subscription stripeSubscription)
        {
            var sub = await _subscriptionRepository.GetByStripeId(stripeSubscription.Id);
            if (sub == null) return;

            var account = await _accountRepository.Get(sub.AccountId);
            if (account == null) return;

            sub.Status = SubscriptionStatus.Cancelled;
            sub.CancelledAt = DateTime.UtcNow;
            account.Tier = SubscriptionTier.Free;

            await _subscriptionRepository.Update(sub);
            await _accountRepository.Update(account);
        }

        public async Task HandlePaymentFailure(Invoice invoice)
        {
            if (invoice.SubscriptionId == null) return;

            var userSubscription = await _subscriptionRepository.GetByStripeId(invoice.SubscriptionId);
            if (userSubscription == null) return;

            var account = await _accountRepository.Get(userSubscription.AccountId);
            if (account == null) return;

            if (invoice.AttemptCount >= 3)
            {
                userSubscription.Status = SubscriptionStatus.Unpaid;
                account.Tier = SubscriptionTier.Free;
                await _accountRepository.Update(account);
            }
            else
            {
                userSubscription.Status = SubscriptionStatus.PastDue;
            }

            await _subscriptionRepository.Update(userSubscription);
        }

        public Task HandleTrialEnding(Subscription stripeSubscription)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> HasActiveSubscription(string accountId)
        {
            var sub = await _subscriptionRepository.GetByAccountId(accountId);
            return sub?.Status is SubscriptionStatus.Active or SubscriptionStatus.Trialing;
        }

        public async Task<bool> IsTrialing(string accountId)
        {
            var sub = await _subscriptionRepository.GetByAccountId(accountId);
            return sub?.Status == SubscriptionStatus.Trialing;
        }

        public async Task SyncSubscription(string subscriptionId)
        {
            var stripeSub = await _stripeSubscriptionService.GetAsync(subscriptionId);
            var sub = await _subscriptionRepository.GetByStripeId(subscriptionId);

            if (stripeSub == null || sub == null) return;

            await UpdateSubscription(stripeSub);
        }

        private static SubscriptionPlan MapPlanType(string plan)
        {
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
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentNullException(nameof(status));

            return status switch
            {
                "active" => SubscriptionStatus.Active,
                "canceled" => SubscriptionStatus.Cancelled,
                "incomplete" => SubscriptionStatus.Incomplete,
                "incomplete_expired" => SubscriptionStatus.IncompleteExpired,
                "past_due" => SubscriptionStatus.PastDue,
                "trialing" => SubscriptionStatus.Trialing,
                "unpaid" => SubscriptionStatus.Unpaid,
                "paused" => SubscriptionStatus.Paused,
                _ => SubscriptionStatus.Unknown
            };
        }

        private static SubscriptionTier MapPlanToTier(SubscriptionPlan plan) => plan switch
        {
            SubscriptionPlan.Pro or SubscriptionPlan.ProYearly => SubscriptionTier.Pro,
            SubscriptionPlan.Ultimate or SubscriptionPlan.UltimateYearly => SubscriptionTier.Ultimate,
            _ => SubscriptionTier.Free
        };
    }
}