using System.ComponentModel.DataAnnotations;

namespace LudereAI.Web.Models;

public class SignUpViewModel
{
    
    [Required(ErrorMessage = "First Name is required")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "Last Name is required")]
    public string LastName { get; set; }
    
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Confirm Password is required")]
    public string ConfirmPassword { get; set; }
}