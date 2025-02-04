using LudereAI.Shared.DTOs;

namespace LudereAI.WPF.Interfaces;

public interface ITokenService
{
    event EventHandler TokenInvalidated;
    Task<AccountDTO?> ValidateToken();
}