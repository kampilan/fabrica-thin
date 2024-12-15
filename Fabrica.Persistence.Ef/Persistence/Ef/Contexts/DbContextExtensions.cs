using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Ef.Contexts;

public static class DbContextExtensions
{

    public static async Task<TEntity?> TrackedOrDefaultAsync<TEntity>(this DbContext context, string uid, CancellationToken cancellationToken = default) where TEntity : class, IEntity
    {

        var entry = context.ChangeTracker.Entries<TEntity>().SingleOrDefault(e => e.Entity.Uid == uid);
        if (entry is not null)
            return entry.Entity;

        var entity = await context.Set<TEntity>().SingleOrDefaultAsync(e => e.Uid == uid, cancellationToken);

        return entity;

    }
    
}