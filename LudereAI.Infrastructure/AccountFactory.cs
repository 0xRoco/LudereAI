using LudereAI.Application.Interfaces;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models.Account;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;

namespace LudereAI.Infrastructure;

public class AccountFactory(ISecurityService securityService) : IAccountFactory
{

    public Account Create(SignUpDTO dto)
    {
        return new Account
        {
            Id = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            DeletedAt = DateTime.MinValue,
            Username = dto.Username,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            HashedPassword = securityService.HashPassword(dto.Password),
            Role = AccountRole.User,
            Status = AccountStatus.Active,
            LastLogin = null, 
            DeviceId = dto.DeviceId,
        };
    }

    public Account Create(GuestDTO dto)
    {
        var id = Guid.NewGuid().ToString("N");
        
        return new Account
        {
            Id = id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            DeletedAt = DateTime.MinValue,
            Username = $"guest_{dto.DeviceId}",
            Email = "guest@LudereAI.com",
            FirstName = "Guest",
            LastName = "User",
            HashedPassword = "",
            Role = AccountRole.User,
            Status = AccountStatus.Active,
            LastLogin = null,
            DeviceId = dto.DeviceId,
        };
    }
}