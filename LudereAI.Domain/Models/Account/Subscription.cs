using System.ComponentModel.DataAnnotations;
using LudereAI.Shared.Enums;

namespace LudereAI.Domain.Models.Account;

public class Subscription : BaseEntity
{
    [Required]
    public string AccountId { get; set; }
    [Required]
    public string StripeId { get; set; }
    
    [Required]
    public SubscriptionPlan SubscriptionPlan { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    [Required]
    public SubscriptionStatus Status { get; set; }
}