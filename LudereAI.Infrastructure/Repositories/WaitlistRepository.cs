using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace LudereAI.Infrastructure.Repositories;

public class WaitlistRepository : IWaitlistRepository
{
    private readonly DatabaseContext _context;

    public WaitlistRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WaitlistEntry>> GetAll()
    {
        return await _context.WaitlistEntries
            .AsNoTracking()
            .OrderBy(w => w.Position)
            .ToListAsync();
    }

    public async Task<IEnumerable<WaitlistEntry>> GetUninvitedBatch(int batchSize)
    {
        return await _context.WaitlistEntries
            .AsNoTracking()
            .Where(w => !w.IsInvited)
            .OrderBy(w => w.Position)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task<WaitlistEntry?> Get(string id)
    {
        return await _context.WaitlistEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<WaitlistEntry?> GetByEmail(string email)
    {
        return await _context.WaitlistEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Email == email);
    }

    public async Task<WaitlistEntry?> GetByPosition(int position)
    {
        return await _context.WaitlistEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Position == position);
    }

    public async Task<WaitlistEntry> Add(WaitlistEntry entry)
    {
        await _context.WaitlistEntries.AddAsync(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<WaitlistEntry> Update(WaitlistEntry entry)
    {
        _context.WaitlistEntries.Entry(entry).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task Delete(string id)
    {
        var entry = await Get(id);
        if (entry is null)
        {
            return;
        }

        _context.WaitlistEntries.Remove(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetNextPosition()
    {
        if (await _context.WaitlistEntries.AnyAsync())
        {
            return await _context.WaitlistEntries.MaxAsync(w => w.Position) + 1;
        }
        
        return 1;
    }
}