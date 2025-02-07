using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Domain.Models.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Repositories;

public class GuestRepository : IGuestRepository
{
    private readonly ILogger<IGuestRepository> _logger;
    private readonly DatabaseContext _context;

    public GuestRepository(ILogger<IGuestRepository> logger, DatabaseContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IEnumerable<GuestAccount>> GetAll()
    {
        return await _context.Guests.AsNoTracking().ToListAsync();
    }

    public async Task<GuestAccount?> Get(string id)
    {
        return await _context.Guests.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GuestAccount?> GetByDeviceId(string deviceId)
    {
        return await _context.Guests.AsNoTracking().FirstOrDefaultAsync(g => g.DeviceId == deviceId);
    }

    public async Task<bool> Create(GuestAccount guestAccount)
    {
        await _context.Guests.AddAsync(guestAccount);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Update(GuestAccount guestAccount)
    {
        _context.Guests.Update(guestAccount);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Delete(string id)
    {
        var guest = await _context.Guests.FindAsync(id);
        if (guest == null) return false;

        _context.Guests.Remove(guest);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Exists(string deviceId)
    {
        return await _context.Guests.AsNoTracking().AnyAsync(g => g.DeviceId == deviceId);
    }
}