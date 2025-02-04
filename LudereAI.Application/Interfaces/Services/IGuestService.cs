using LudereAI.Domain.Models.Account;
using LudereAI.Shared.DTOs;

namespace LudereAI.Application.Interfaces.Services;

public interface IGuestService
{
    Task<GuestAccount?> GetGuestAccount(string accountId);
    Task<GuestAccount?> GetGuestAccountByDeviceId(string deviceId);
    Task<AccountDTO?> CreateGuestAccount(GuestDTO dto);
    Task<bool> DeleteGuestAccount(string username);
}