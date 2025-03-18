using System.ComponentModel.DataAnnotations;
using LudereAI.Domain.Models.Chat;
using LudereAI.Shared.Enums;

namespace LudereAI.Domain.Models.Account;

public class Account : BaseEntity
{
    [Required, MaxLength(50)]
    public string Username { get; set; } = "";
    
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = "";
    
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = "";
    
    [Required, MaxLength(100)]
    public string LastName { get; set; } = "";
    
    [Required]
    public string HashedPassword { get; set; } = "";
    public AccountRole Role { get; set; } = AccountRole.User;
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;
    
    public DateTime? LastLogin { get; set; }
    public string DeviceId { get; set; } = "";
    public string? StripeCustomerId { get; set; } = "";
    
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsSubscribed => Tier is SubscriptionTier.Pro or SubscriptionTier.Ultimate;
    
    public UserSubscription? Subscription { get; set; }
    public IEnumerable<Conversation>? Conversations { get; set; } = new List<Conversation>();
}