using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Account;
using LudereAI.Shared.DTOs;

namespace LudereAI.Application.Interfaces.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginDTO dto);
    Task<bool> RegisterAsync(SignUpDTO dto);
    string GenerateJWT(AccountDTO account);
    string GenerateGuestJWT(AccountDTO dto);
}