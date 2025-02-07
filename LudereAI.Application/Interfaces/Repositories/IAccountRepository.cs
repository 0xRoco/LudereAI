using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Account;

namespace LudereAI.Application.Interfaces.Repositories;

public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAll();
    Task<Account?> Get(string accountId);
    Task<Account?> GetByUsername(string username);
    Task<Account?> GetByEmail(string email);
    Task<bool> Create(Account account);
    Task<bool> Update(Account account);
    Task<bool> Delete(string accountId);

    Task<bool> UpdateLastLogin(string accountId);
}