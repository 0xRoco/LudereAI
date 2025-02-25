using System.ComponentModel.DataAnnotations;

namespace LudereAI.Shared.DTOs.Waitlist;

public class JoinWaitlistDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}