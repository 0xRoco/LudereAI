namespace LudereAI.Domain.Models.Account;

public class GuestAccount : BaseEntity
{ 
    public string AccountId { get; set; } = "";
    public string DeviceId { get; set; } = "";
}