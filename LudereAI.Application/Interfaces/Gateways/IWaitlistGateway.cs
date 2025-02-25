using LudereAI.Domain.Models;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.DTOs.Waitlist;

namespace LudereAI.Application.Interfaces.Gateways;

public interface IWaitlistGateway
{
    Task<IEnumerable<WaitlistEntry>> GetAll();
    Task<WaitlistEntry?> GetByEmail(string email);
    Task<JoinedWaitlistDTO> JoinWaitlist(string email);
    Task<bool> Invite(string email);
    Task<bool> InviteNextBatch(int batchSize);
    Task<bool> RemoveFromWaitlist(string email);
}