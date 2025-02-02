using System.ComponentModel.DataAnnotations;

namespace LudereAI.Shared.DTOs;

public class UpdateAccountDTO
{
    public string NewUsername { get; set; }
    
    [EmailAddress]
    public string NewEmail { get; set; }
    
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; }
}