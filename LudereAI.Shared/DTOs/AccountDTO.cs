using LudereAI.Shared.Enums;

namespace LudereAI.Shared.DTOs;

public class AccountDTO
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DeviceId { get; set; }
    public AccountRole Role { get; set; }
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    public IEnumerable<ConversationDTO> Conversations { get; set; }
}