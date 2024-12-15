using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Ef.Contexts;

public static class DbContextExtensions
{

    public static async Task<T?> LocalSingleOrDefault<T>(this DbContext context, string uid, CancellationToken cancellationToken = default) where T : class, IEntity
    {

        var entry = context.ChangeTracker.Entries<T>().SingleOrDefault(e => e.Entity.Uid == uid);
        if (entry is not null)
            return entry.Entity;

        var entity = await context.Set<T>().SingleOrDefaultAsync(e => e.Uid == uid, cancellationToken);

        return entity;

    }
    
}