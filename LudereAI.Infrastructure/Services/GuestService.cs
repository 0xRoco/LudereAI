using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models.Account;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Services;

public class GuestService : IGuestService
{
    private readonly ILogger<IGuestService> _logger;
    private readonly IGuestRepository _guestRepository;
    private readonly IAccountService _accountService;

    public GuestService(ILogger<IGuestService> logger, IGuestRepository guestRepository, IAccountService accountService)
    {
        _logger = logger;
        _guestRepository = guestRepository;
        _accountService = accountService;
    }

    public async Task<AccountDTO?> CreateGuestAccount(GuestDTO dto)
    {
        var existingAccount = await _guestRepository.GetByDeviceId(dto.DeviceId);
        if (existingAccount != null)
        {
            var accountDTO = await _accountService.GetAccount(existingAccount.AccountId);
            return accountDTO;
        }
        
        var account = await _accountService.CreateGuestAccount(new GuestDTO {DeviceId = dto.DeviceId});
        if (account == null) return null;
        
        var guestAccount = new GuestAccount
        {
            Id = Guid.NewGuid().ToString("N"),
            AccountId = account.Id,
            DeviceId = dto.DeviceId,
            CreatedAt = DateTime.UtcNow
        };
        
        await _guestRepository.Create(guestAccount);
        return account;
    }

    public async Task<GuestAccount?> GetGuestAccount(string username)
    {
        return await _guestRepository.Get(username);
    }

    public async Task<GuestAccount?> GetGuestAccountByDeviceId(string deviceId)
    {
        return await _guestRepository.GetByDeviceId(deviceId);
    }

    public async Task<bool> DeleteGuestAccount(string username)
    {
        var guestAccount = await _guestRepository.Get(username);
        if (guestAccount == null) return false;

        await _accountService.DeleteAccount(guestAccount.AccountId);
        return true;
    }
}