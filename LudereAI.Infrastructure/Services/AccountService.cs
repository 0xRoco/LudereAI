using AutoMapper;
using LudereAI.Application.Interfaces;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly ILogger<IAccountService> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountFactory _accountFactory;
    private readonly IMapper _mapper;

    public AccountService(ILogger<IAccountService> logger,
        IAccountRepository accountRepository,
        IMapper mapper, IAccountFactory accountFactory)
    {
        _logger = logger;
        _accountRepository = accountRepository;
        _mapper = mapper;
        _accountFactory = accountFactory;
    }

    public async Task<IEnumerable<AccountDTO>> GetAccounts()
    {
        var accounts = await _accountRepository.GetAll();
        return _mapper.Map<IEnumerable<AccountDTO>>(accounts);
    }

    public async Task<AccountDTO?> GetAccount(string id)
    {
        var account = await _accountRepository.Get(id);
        return _mapper.Map<AccountDTO>(account);
    }

    public async Task<AccountDTO?> GetAccountByUsername(string username)
    {
        var account = await _accountRepository.GetByUsername(username);
        return _mapper.Map<AccountDTO>(account);
    }

    public async Task<AccountDTO?> GetAccountByEmail(string email)
    {
        var account = await _accountRepository.GetByEmail(email);
        return _mapper.Map<AccountDTO>(account);
    }

    public async Task<bool> AccountExists(string username)
    {
        return await _accountRepository.GetByUsername(username) != null;
    }

    public async Task<AccountDTO?> CreateAccount(SignUpDTO dto)
    {
        var account = _accountFactory.Create(dto);
        
        if (!await _accountRepository.Create(account)) return null;
        
        return _mapper.Map<AccountDTO>(account);
    }

    public async Task<AccountDTO?> CreateGuestAccount(GuestDTO dto)
    {
        var account = _accountFactory.Create(dto);
        if (!await _accountRepository.Create(account)) return null;
        
        //await _stripeService.CreateOrSyncAccount(account);
        return _mapper.Map<AccountDTO>(account);
    }

    public async Task<AccountDTO?> UpdateAccount(string id, UpdateAccountDTO dto)
    {
        var account = await _accountRepository.Get(id);
        if (account == null) return null;

        account = _mapper.Map(dto, account);

        if (!await _accountRepository.Update(account)) return null;
        
        return _mapper.Map<AccountDTO>(account);
    }

    public async Task<bool> DeleteAccount(string id)
    {
        return await _accountRepository.Delete(id);
    }

    public async Task<bool> UpdateLastLogin(string accountId)
    {
        return await _accountRepository.UpdateLastLogin(accountId);
    }
}