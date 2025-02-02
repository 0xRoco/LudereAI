using AutoMapper;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Account;
using LudereAI.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Services;

public class AccountService(ILogger<IAccountService> logger,
    IAccountRepository accountRepository,
    ISecurityService securityService,
    IStripeService stripeService,
    IMapper mapper) : IAccountService
{
    public async Task<AccountDTO?> GetAccount(string id)
    {
        var account = await accountRepository.GetAsync(id);
        return mapper.Map<AccountDTO>(account);
    }

    public async Task<AccountDTO?> GetAccountByUsername(string username)
    {
        var account = await accountRepository.GetByUsernameAsync(username);
        return mapper.Map<AccountDTO>(account);
    }

    public async Task<AccountDTO?> GetAccountByEmail(string email)
    {
        var account = await accountRepository.GetByEmailAsync(email);
        return mapper.Map<AccountDTO>(account);
    }

    public async Task<bool> AccountExists(string username)
    {
        return await accountRepository.GetByUsernameAsync(username) != null;
    }

    public async Task<AccountDTO?> CreateAccount(SignUpDTO dto)
    {
        var account = mapper.Map<Account>(dto);
        account.HashedPassword = securityService.HashPassword(dto.Password);

        if (!await accountRepository.CreateAsync(account)) return null;
        
        await stripeService.CreateOrSyncAccount(account);
        return mapper.Map<AccountDTO>(account);

    }

    public async Task<AccountDTO?> UpdateAccount(string id, UpdateAccountDTO dto)
    {
        var account = await accountRepository.GetAsync(id);
        if (account == null) return null;

        account = mapper.Map(dto, account);

        if (!await accountRepository.UpdateAsync(account)) return null;
        
        await stripeService.CreateOrSyncAccount(account);
        return mapper.Map<AccountDTO>(account);

    }

    public async Task<bool> DeleteAccount(string id)
    {
        return await accountRepository.DeleteAsync(id);
    }
}