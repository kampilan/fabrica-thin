using System.Linq.Expressions;
using Autofac;
using Fabrica.Exceptions;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Utilities;
using Microsoft.EntityFrameworkCore;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Persistence.Repository;


internal static class Helpers
{
    internal static readonly HashSet<EntityState> DirtyStates = [EntityState.Added, EntityState.Deleted, EntityState.Modified];
}


public static class AutofacExtensions
{


    public static ContainerBuilder RegisterRepositories<TOrigin, TReplica>(this ContainerBuilder builder) where TOrigin : DbContext where TReplica : DbContext
    {


        builder.Register(c =>
        {
            var ctx = c.Resolve<TOrigin>();
            var comp = new OriginDbContextFactory(ctx);
            return comp;
        })
            .As<IOriginDbContextFactory>()
            .InstancePerDependency();


        builder.Register(c =>
        {
            var ctx = c.Resolve<TReplica>();
            var comp = new ReplicaDbContextFactory(ctx);
            return comp;
        })
            .As<IReplicaDbContextFactory>()
            .InstancePerDependency();


        builder.Register(c =>
        {
            var corr    = c.Resolve<ICorrelation>();
            var factory = c.Resolve<IOriginDbContextFactory>();
            var rules   = c.Resolve<IRuleSet>();

            var comp = new CommandRepository( corr, factory, rules );

            return comp;

        })
            .As<ICommandRepository>()
            .InstancePerLifetimeScope();


        builder.Register(c =>
        {

            var corr = c.Resolve<ICorrelation>();
            var factory = c.Resolve<IReplicaDbContextFactory>();

            var comp = new QueryRepository( corr, factory );

            return comp;

        })
            .As<IQueryRepository>()
            .InstancePerLifetimeScope();



        return builder;

    }


}



public interface ICommandRepository
{

    Task<ListOrError<TEntity>> Many<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity;

    Task<EntityOrError<TEntity>> One<TEntity>(long id, CancellationToken ct = default) where TEntity : class, IEntity;
    Task<EntityOrError<TEntity>> One<TEntity>(string uid, CancellationToken ct = default) where TEntity : class, IEntity;
    Task<EntityOrError<TEntity>> One<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity;

    Task<EntityOrError<TEntity>> Eager<TEntity>(Func<DbSet<TEntity>, IQueryable<TEntity>> queryable, long id, CancellationToken ct = default) where TEntity : class, IEntity;
    Task<EntityOrError<TEntity>> Eager<TEntity>(Func<DbSet<TEntity>, IQueryable<TEntity>> queryable, string uid, CancellationToken ct = default) where TEntity : class, IEntity;
    Task<EntityOrError<TEntity>> Eager<TEntity>(Func<DbSet<TEntity>, IQueryable<TEntity>> queryable, Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity;

    Task<OkOrError> Add<TEntity>( TEntity entity, CancellationToken ct = default) where TEntity : class, IEntity;
    Task<OkOrError> AddRange(IEnumerable<IEntity> range, CancellationToken ct = default);


    Task<OkOrError> Delete<TEntity>(long id, CancellationToken ct = default) where TEntity : class, IEntity;
    Task<OkOrError> Delete<TEntity>(string uid, CancellationToken ct = default) where TEntity : class, IEntity;
    OkOrError DeleteRange( IEnumerable<IEntity> range );

    Task<OkOrError> Save(CancellationToken ct = default);

}

public class CommandRepository( ICorrelation correlation, IOriginDbContextFactory factory, IRuleSet rules ): CorrelatedObject(correlation), ICommandRepository
{

    private DbContext Context => factory.GetDbContext();


    public async Task<ListOrError<TEntity>> Many<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        try
        {

            var list = await Context.Set<TEntity>().Where(predicate).ToListAsync(ct);

            return list;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName() };
            logger.ErrorWithContext(cause, ctx, "Many failed");

            return UnhandledError.Create(cause);

        }

    }

    public async Task<EntityOrError<TEntity>> One<TEntity>(long id, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        try
        {

            var entity = await Context.Set<TEntity>().FindAsync(id, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using Id={id}");

            return entity;

        }
        catch (Exception cause)
        {

            var ctx = new {Type = typeof(TEntity).GetConciseFullName(), Id = id};
            logger.ErrorWithContext( cause, ctx, "One by Id failed" );

           return UnhandledError.Create(cause);

        }


    }

    public async Task<EntityOrError<TEntity>> One<TEntity>(string uid, CancellationToken ct = default) where TEntity : class, IEntity
    {


        using var logger = EnterMethod();

        try
        {

            var entity = await Context.Set<TEntity>().SingleOrDefaultAsync(m => m.Uid == uid, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using Uid=({uid})");

            return entity;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName(), Uid = uid };
            logger.ErrorWithContext(cause, ctx, "One by Uid failed");

            return UnhandledError.Create(cause);

        }

    }


    public async Task<EntityOrError<TEntity>> One<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {


        using var logger = EnterMethod();

        try
        {

            var entity = await Context.Set<TEntity>().SingleOrDefaultAsync(predicate, ct);
            if( entity is null )
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using predicate");

            return entity;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName() };
            logger.ErrorWithContext(cause, ctx, "One by predicate failed");

            return UnhandledError.Create(cause);

        }


    }



    public async Task<EntityOrError<TEntity>> Eager<TEntity>(Func<DbSet<TEntity>, IQueryable<TEntity>> extractor, long id, CancellationToken ct = default) where TEntity : class, IEntity
    {


        using var logger = EnterMethod();

        try
        {

            var queryable = extractor(Context.Set<TEntity>());

            var entity = await queryable.SingleOrDefaultAsync(e => e.Id == id, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using Id={id}");

            return entity;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName(), Id=id };
            logger.ErrorWithContext(cause, ctx, "Eager by Id failed");

            return UnhandledError.Create(cause);

        }


    }

    public async Task<EntityOrError<TEntity>> Eager<TEntity>(Func<DbSet<TEntity>, IQueryable<TEntity>> extractor, string uid, CancellationToken ct = default) where TEntity : class, IEntity
    {


        using var logger = EnterMethod();

        try
        {

            var queryable = extractor(Context.Set<TEntity>());

            var entity = await queryable.SingleOrDefaultAsync(e => e.Uid == uid, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using Uid={uid}");

            return entity;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName(), Uid = uid };
            logger.ErrorWithContext(cause, ctx, "Eager by Uid failed");

            return UnhandledError.Create(cause);

        }


    }

    public async Task<EntityOrError<TEntity>> Eager<TEntity>(Func<DbSet<TEntity>, IQueryable<TEntity>> extractor, Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {


        using var logger = EnterMethod();

        try
        {

            var queryable = extractor(Context.Set<TEntity>());

            var entity = await queryable.SingleOrDefaultAsync(predicate, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using predicate");

            return entity;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName() };
            logger.ErrorWithContext(cause, ctx, "Eager by predicate failed");

            return UnhandledError.Create(cause);

        }


    }


    public async Task<OkOrError> Add<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        try
        {
            await Context.AddAsync(entity, ct);

            return Ok.Singleton;

        }
        catch (Exception cause)
        {
            var ctx = new { Type = typeof(TEntity).GetConciseFullName(), entity.Uid };
            logger.ErrorWithContext(cause, ctx, "Add failed");

            return UnhandledError.Create(cause);

        }

    }

    public async Task<OkOrError> AddRange(IEnumerable<IEntity> range, CancellationToken ct = default)
    {

        using var logger = EnterMethod();

        try
        {
            await Context.AddRangeAsync(range, ct);

            return Ok.Singleton;

        }
        catch (Exception cause)
        {
            logger.Error(cause, "Add range failed");

            return UnhandledError.Create(cause);

        }

    }



    public async Task<OkOrError> Delete<TEntity>(long id, CancellationToken ct = default) where TEntity : class, IEntity
    {


        using var logger = EnterMethod();


        try
        {

            var entity = await Context.Set<TEntity>().FindAsync(id, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using Id={id}");


            Context.Remove(entity);

            return Ok.Singleton;

        }
        catch (Exception cause)
        {
            var ctx = new { Type = typeof(TEntity).GetConciseFullName(), Id=id };
            logger.ErrorWithContext(cause, ctx, "Delete by Id failed");

            return UnhandledError.Create(cause);

        }

    }

    public async Task<OkOrError> Delete<TEntity>(string uid, CancellationToken ct = default) where TEntity : class, IEntity
    {


        using var logger = EnterMethod();


        try
        {

            var entity = await Context.Set<TEntity>().SingleOrDefaultAsync(m => m.Uid == uid, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using Uid=({uid})");


            Context.Remove(entity);

            return Ok.Singleton;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName(), Uid = uid };
            logger.ErrorWithContext(cause, ctx, "Delete by Uid failed");

            return UnhandledError.Create(cause);

        }


    }

    public OkOrError DeleteRange( IEnumerable<IEntity> range )
    {

        using var logger = EnterMethod();

        try
        {

            Context.RemoveRange(range);

            return Ok.Singleton;

        }
        catch (Exception cause)
        {

            logger.Error(cause,  "Delete range failed");

            return UnhandledError.Create(cause);

        }

    }



    public async Task<OkOrError> Save(CancellationToken ct = default)
    {

        using var logger = EnterMethod();


        var dirty = Context.ChangeTracker.Entries().Where(e => Helpers.DirtyStates.Contains(e.State)).Select(d=>d.Entity).ToList();

        logger.LogObject(nameof(dirty.Count), dirty.Count );


        if( !rules.TryValidate(dirty, out var violations) )
            return NotValidError.Create(violations);


        try
        {

            var affected = await Context.SaveChangesAsync(ct);

            logger.Inspect(nameof(affected), affected);


            return Ok.Singleton;

        }
        catch (Exception cause)
        {
            logger.Error(cause, "Save failed.");

            return UnhandledError.Create(cause);

        }


    }



}


public interface IOriginDbContextFactory
{
    DbContext GetDbContext();
}

public class OriginDbContextFactory(DbContext context) : IOriginDbContextFactory
{
    public DbContext GetDbContext() => context;

}



public interface IQueryRepository
{

    Task<ListOrError<TEntity>> Many<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity;

    Task<EntityOrError<TEntity>> One<TEntity>(long id, CancellationToken ct = default) where TEntity : class, IEntity;
    Task<EntityOrError<TEntity>> One<TEntity>(string uid, CancellationToken ct = default) where TEntity : class, IEntity;
    Task<EntityOrError<TEntity>> One<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity;

    Task<EntityOrError<TEntity>> Eager<TEntity>(Func<DbSet<TEntity>, IQueryable<TEntity>> queryable, long id, CancellationToken ct = default) where TEntity : class, IEntity;
    Task<EntityOrError<TEntity>> Eager<TEntity>(Func<DbSet<TEntity>, IQueryable<TEntity>> queryable, string uid, CancellationToken ct = default) where TEntity : class, IEntity;
    Task<EntityOrError<TEntity>> Eager<TEntity>(Func<DbSet<TEntity>, IQueryable<TEntity>> queryable, Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity;


}

public class QueryRepository( ICorrelation correlation, IReplicaDbContextFactory factory ) : CorrelatedObject(correlation), IQueryRepository
{

    private DbContext Context => factory.GetDbContext();


    public async Task<ListOrError<TEntity>> Many<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        try
        {

            var list = await Context.Set<TEntity>().Where(predicate).ToListAsync(ct);

            return list;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName() };
            logger.ErrorWithContext(cause, ctx, "Many failed");

            return UnhandledError.Create(cause);

        }

    }

    public async Task<EntityOrError<TEntity>> One<TEntity>(long id, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        try
        {

            var entity = await Context.Set<TEntity>().FindAsync(id, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using Id={id}");

            return entity;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName(), Id = id };
            logger.ErrorWithContext(cause, ctx, "One by Id failed");

            return UnhandledError.Create(cause);

        }


    }

    public async Task<EntityOrError<TEntity>> One<TEntity>(string uid, CancellationToken ct = default) where TEntity : class, IEntity
    {


        using var logger = EnterMethod();

        try
        {

            var entity = await Context.Set<TEntity>().SingleOrDefaultAsync(m => m.Uid == uid, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using Uid=({uid})");

            return entity;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName(), Uid = uid };
            logger.ErrorWithContext(cause, ctx, "One by Uid failed");

            return UnhandledError.Create(cause);

        }

    }


    public async Task<EntityOrError<TEntity>> One<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {


        using var logger = EnterMethod();

        try
        {

            var entity = await Context.Set<TEntity>().SingleOrDefaultAsync(predicate, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using predicate");

            return entity;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName() };
            logger.ErrorWithContext(cause, ctx, "One by predicate failed");

            return UnhandledError.Create(cause);

        }


    }



    public async Task<EntityOrError<TEntity>> Eager<TEntity>(Func<DbSet<TEntity>, IQueryable<TEntity>> extractor, long id, CancellationToken ct = default) where TEntity : class, IEntity
    {


        using var logger = EnterMethod();

        try
        {

            var queryable = extractor(Context.Set<TEntity>());

            var entity = await queryable.SingleOrDefaultAsync(e => e.Id == id, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using Id={id}");

            return entity;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName(), Id = id };
            logger.ErrorWithContext(cause, ctx, "Eager by Id failed");

            return UnhandledError.Create(cause);

        }


    }

    public async Task<EntityOrError<TEntity>> Eager<TEntity>(Func<DbSet<TEntity>, IQueryable<TEntity>> extractor, string uid, CancellationToken ct = default) where TEntity : class, IEntity
    {


        using var logger = EnterMethod();

        try
        {

            var queryable = extractor(Context.Set<TEntity>());

            var entity = await queryable.SingleOrDefaultAsync(e => e.Uid == uid, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using Uid={uid}");

            return entity;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName(), Uid = uid };
            logger.ErrorWithContext(cause, ctx, "Eager by Uid failed");

            return UnhandledError.Create(cause);

        }


    }

    public async Task<EntityOrError<TEntity>> Eager<TEntity>(Func<DbSet<TEntity>, IQueryable<TEntity>> extractor, Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {


        using var logger = EnterMethod();

        try
        {

            var queryable = extractor(Context.Set<TEntity>());

            var entity = await queryable.SingleOrDefaultAsync(predicate, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {typeof(TEntity).GetConciseName()} using predicate");

            return entity;

        }
        catch (Exception cause)
        {

            var ctx = new { Type = typeof(TEntity).GetConciseFullName() };
            logger.ErrorWithContext(cause, ctx, "Eager by predicate failed");

            return UnhandledError.Create(cause);

        }


    }




}


public interface IReplicaDbContextFactory
{
    DbContext GetDbContext();
}

public class ReplicaDbContextFactory(DbContext context) : IReplicaDbContextFactory
{
    public DbContext GetDbContext() => context;

}








