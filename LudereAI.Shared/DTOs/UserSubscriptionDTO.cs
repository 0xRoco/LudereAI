using LudereAI.Shared.Enums;

namespace LudereAI.Shared.DTOs;

public class UserSubscriptionDTO
{
    public string Id { get; set; }
    public SubscriptionPlan Plan { get; set; }
    public SubscriptionStatus Status { get; set; }

    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    public string PlanFriendlyName => Plan switch
    {
        SubscriptionPlan.Pro => "Pro",
        SubscriptionPlan.ProYearly => "Pro (Yearly)",
        SubscriptionPlan.Ultimate => "Ultimate",
        SubscriptionPlan.UltimateYearly => "Ultimate (Yearly)",
        _ => "Free"
    };
    
    public string StatusFriendlyName => Status switch
    {
        SubscriptionStatus.Active => "Active",
        SubscriptionStatus.Trialing => "Trialing",
        SubscriptionStatus.PastDue => "Past Due",
        SubscriptionStatus.Cancelled => "Cancelled",
        SubscriptionStatus.Incomplete => "Incomplete",
        SubscriptionStatus.IncompleteExpired => "Incomplete Expired",
        SubscriptionStatus.Unpaid => "Unpaid",
        _ => "Unknown"
    };

    public string FriendlyEndDate()
    {
        if (CancelAtPeriodEnd && Status == SubscriptionStatus.Active) return "Ends on " + CurrentPeriodEnd.ToString("MMMM dd, yyyy");

        return Status switch
        {
            SubscriptionStatus.Active => "Renews on " + CurrentPeriodEnd.ToString("MMMM dd, yyyy"),
            SubscriptionStatus.Trialing => "Trial ends on " + CurrentPeriodEnd.ToString("MMMM dd, yyyy"),
            SubscriptionStatus.Cancelled => "Cancelled on " + CancelledAt?.ToString("MMMM dd, yyyy"),
            _ => "Ends on " + CurrentPeriodEnd.ToString("MMMM dd, yyyy")
        };
    }
}