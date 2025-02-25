using System.ComponentModel.DataAnnotations;

namespace LudereAI.Shared.DTOs;

public class LoginDTO
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
    
    public LoginDTO(){}
}