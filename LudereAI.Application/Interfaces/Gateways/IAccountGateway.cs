using LudereAI.Shared;
using LudereAI.Shared.DTOs;

namespace LudereAI.Application.Interfaces.Gateways;

public interface IAccountGateway
{
    Task<Result<IEnumerable<AccountDTO>, AccountOperationResult>> GetAccounts();
    Task<Result<AccountDTO, AccountOperationResult>> GetAccount(string id);
    Task<Result<AccountDTO, AccountOperationResult>> GetAccountByUsername(string username);
    Task<Result<AccountDTO, AccountOperationResult>> GetAccountByEmail(string email);
    Task<Result<bool, AccountOperationResult>> AccountExists(string username);
    Task<Result<AccountDTO, AccountOperationResult>> CreateAccount(SignUpDTO dto);
    Task<Result<AccountDTO, AccountOperationResult>> CreateGuestAccount(GuestDTO dto);
    Task<Result<AccountDTO, AccountOperationResult>> UpdateAccount(string id, UpdateAccountDTO dto);
    Task<Result<bool, AccountOperationResult>> DeleteAccountAsync(string id);
    
    
    public enum AccountOperationResult
    {
        Success,
        NotFound,
        AlreadyExists,
        Error
    }
}