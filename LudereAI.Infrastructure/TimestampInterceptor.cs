using LudereAI.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LudereAI.Infrastructure;

public class TimestampInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        var entries = eventData.Context?.ChangeTracker.Entries();
        if (entries == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (var entry in entries)
        {
            if (entry.Entity is BaseEntity entity)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entity.UpdatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entity.DeletedAt = DateTime.UtcNow;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(typeof(EntityState).ToString());
                }
            }
        }
        
        
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}