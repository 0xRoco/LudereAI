using LudereAI.Domain.Models.Account;

namespace LudereAI.Application.Interfaces.Repositories;

public interface IGuestRepository
{
    Task<IEnumerable<GuestAccount>> GetAll();
    Task<GuestAccount?> Get(string id);
    Task<GuestAccount?> GetByDeviceId(string deviceId);
    
    Task<bool> Create(GuestAccount guestAccount);
    Task<bool> Update(GuestAccount guestAccount);
    Task<bool> Delete(string id);
    
    Task<bool> Exists(string deviceId);
}