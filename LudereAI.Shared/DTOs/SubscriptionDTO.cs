using LudereAI.Shared.Enums;

namespace LudereAI.Shared.DTOs;

public class SubscriptionDTO
{
    public string Id { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SubscriptionStatus Status { get; set; }
}