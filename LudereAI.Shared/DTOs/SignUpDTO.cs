namespace LudereAI.Shared.DTOs;

public class SignUpDTO
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string DeviceId { get; set; }
}