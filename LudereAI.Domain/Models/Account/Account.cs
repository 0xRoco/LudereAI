﻿using System.ComponentModel.DataAnnotations;
using LudereAI.Domain.Models.Chat;

namespace LudereAI.Domain.Models.Account;

public class Account : BaseEntity
{
    [Required, MaxLength(50)]
    public string Username { get; set; }
    
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; }
    
    [Required, MaxLength(100)]
    public string FirstName { get; set; }
    
    [Required, MaxLength(100)]
    public string LastName { get; set; }
    
    [Required]
    public string HashedPassword { get; set; }
    
    public DateTime? LastLogin { get; set; }
    public bool IsSubscribed { get; set; }

    public string? StripeCustomerId { get; set; } = "";
    
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    public Subscription? Subscription { get; set; }
    public IEnumerable<Conversation> Conversations { get; set; } = new List<Conversation>();
}