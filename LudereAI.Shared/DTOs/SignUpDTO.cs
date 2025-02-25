using System.ComponentModel.DataAnnotations;

namespace LudereAI.Shared.DTOs;

public class SignUpDTO
{
    [Required(ErrorMessage = "First Name is required")]
    public required string FirstName { get; set; }
    
    [Required(ErrorMessage = "Last Name is required")]
    public required string LastName { get; set; }
    
    [Required(ErrorMessage = "Username is required")]
    public required string Username { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public required string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
    public required string DeviceId { get; set; }
}