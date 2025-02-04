using LudereAI.Shared.DTOs;

namespace LudereAI.Application.Interfaces.Services;

public interface IAccountService
{
    Task<AccountDTO?> GetAccount(string id);
    Task<AccountDTO?> GetAccountByUsername(string username);
    Task<AccountDTO?> GetAccountByEmail(string email);
    Task<bool> AccountExists(string username);
    Task<AccountDTO?> CreateAccount(SignUpDTO dto);
    Task<AccountDTO?> CreateGuestAccount(GuestDTO dto); 
    Task<AccountDTO?> UpdateAccount(string id, UpdateAccountDTO dto);
    Task<bool> DeleteAccount(string id);
}