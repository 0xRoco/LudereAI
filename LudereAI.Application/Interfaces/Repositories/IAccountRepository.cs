using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Account;

namespace LudereAI.Application.Interfaces.Repositories;

public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAllAsync();
    Task<Account?> GetAsync(string accountId);
    Task<Account?> GetByUsernameAsync(string username);
    Task<Account?> GetByEmailAsync(string email);
    Task<bool> CreateAsync(Account account);
    Task<bool> UpdateAsync(Account account);
    Task<bool> DeleteAsync(string accountId);
}