using LudereAI.Domain.Models;

namespace LudereAI.Application.Interfaces.Services;

public interface IWaitlistService
{
    Task<IEnumerable<WaitlistEntry>> GetAll();
    Task<WaitlistEntry?> GetByEmail(string email);
    Task<WaitlistEntry> JoinWaitlist(string email);
    Task<bool> Invite(string email);
    Task<bool> InviteNextBatch(int batchSize);
    Task<bool> RemoveFromWaitlist(string email);
}