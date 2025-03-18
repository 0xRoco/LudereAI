using LudereAI.Domain.Models;

namespace LudereAI.Application.Interfaces.Repositories;

public interface IWaitlistRepository
{
    Task<IEnumerable<WaitlistEntry>> GetAll();
    Task<IEnumerable<WaitlistEntry>> GetUninvitedBatch(int batchSize);
    
    Task<WaitlistEntry?> Get(string id);
    Task<WaitlistEntry?> GetByEmail(string email);
    Task<WaitlistEntry?> GetByPosition(int position);
    
    Task<WaitlistEntry> Add(WaitlistEntry entry);
    Task<WaitlistEntry> Update(WaitlistEntry entry);
    Task Delete (string id);
    
    Task<int> GetNextPosition();
}