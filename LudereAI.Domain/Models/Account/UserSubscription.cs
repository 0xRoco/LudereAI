using System.ComponentModel.DataAnnotations;
using LudereAI.Shared.Enums;

namespace LudereAI.Domain.Models.Account;

public class UserSubscription : BaseEntity
{
    public string AccountId { get; set; }
    public string StripeCustomerId { get; set; }
    public string StripeSubscriptionId { get; set; }
    public SubscriptionPlan Plan { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CancelledAt { get; set; }
}