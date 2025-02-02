using LudereAI.Application.Interfaces;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Account;
using LudereAI.Shared.DTOs;

namespace LudereAI.Infrastructure;

public class AccountFactory(ISecurityService securityService) : IAccountFactory
{

    public Account Create(SignUpDTO dto)
    {
        return new Account
        {
            Username = dto.Username,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            HashedPassword = securityService.HashPassword(dto.Password),
        };
    }
}