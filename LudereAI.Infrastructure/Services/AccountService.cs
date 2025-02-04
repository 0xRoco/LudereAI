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
    private readonly ISecurityService _securityService;
    private readonly IStripeService _stripeService;
    private readonly IAccountFactory _accountFactory;
    private readonly IMapper _mapper;

    public AccountService(ILogger<IAccountService> logger,
        IAccountRepository accountRepository,
        ISecurityService securityService,
        IStripeService stripeService,
        IMapper mapper, IAccountFactory accountFactory)
    {
        _logger = logger;
        _accountRepository = accountRepository;
        _securityService = securityService;
        _stripeService = stripeService;
        _mapper = mapper;
        _accountFactory = accountFactory;
    }

    public async Task<AccountDTO?> GetAccount(string id)
    {
        var account = await _accountRepository.GetAsync(id);
        return _mapper.Map<AccountDTO>(account);
    }

    public async Task<AccountDTO?> GetAccountByUsername(string username)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        return _mapper.Map<AccountDTO>(account);
    }

    public async Task<AccountDTO?> GetAccountByEmail(string email)
    {
        var account = await _accountRepository.GetByEmailAsync(email);
        return _mapper.Map<AccountDTO>(account);
    }

    public async Task<bool> AccountExists(string username)
    {
        return await _accountRepository.GetByUsernameAsync(username) != null;
    }

    public async Task<AccountDTO?> CreateAccount(SignUpDTO dto)
    {
        var account = _accountFactory.Create(dto);
        
        if (!await _accountRepository.CreateAsync(account)) return null;
        
        await _stripeService.CreateOrSyncAccount(account);
        return _mapper.Map<AccountDTO>(account);

    }

    public async Task<AccountDTO?> CreateGuestAccount(GuestDTO dto)
    {
        var account = _accountFactory.Create(dto);
        if (!await _accountRepository.CreateAsync(account)) return null;
        
        //await _stripeService.CreateOrSyncAccount(account);
        return _mapper.Map<AccountDTO>(account);
    }

    public async Task<AccountDTO?> UpdateAccount(string id, UpdateAccountDTO dto)
    {
        var account = await _accountRepository.GetAsync(id);
        if (account == null) return null;

        account = _mapper.Map(dto, account);

        if (!await _accountRepository.UpdateAsync(account)) return null;
        
        await _stripeService.CreateOrSyncAccount(account);
        return _mapper.Map<AccountDTO>(account);

    }

    public async Task<bool> DeleteAccount(string id)
    {
        return await _accountRepository.DeleteAsync(id);
    }
}