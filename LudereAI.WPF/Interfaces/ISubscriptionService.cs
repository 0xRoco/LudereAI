using LudereAI.Shared.Enums;

namespace LudereAI.WPF.Interfaces;

public interface ISubscriptionService
{
    Task Subscribe(SubscriptionPlan subscriptionPlan);
    Task CancelSubscription();
}