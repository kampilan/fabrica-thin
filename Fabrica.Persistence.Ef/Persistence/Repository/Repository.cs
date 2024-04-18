using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using Fabrica.Exceptions;
using Fabrica.Identity;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
    Task<ListOrError<TEntity>> Many<TEntity>(Expression<Func<TEntity, bool>> predicate, int limit, CancellationToken ct = default) where TEntity : class, IEntity;

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

    public async Task<ListOrError<TEntity>> Many<TEntity>(Expression<Func<TEntity, bool>> predicate, int limit, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        logger.Debug("For: {0} with Limit: {1}", typeof(TEntity).GetConciseFullName(), limit);

        try
        {



            // *****************************************************************
            logger.Debug("Attempting to build queryable");
            var query = Context.Set<TEntity>().Where(predicate);
            if (limit > 0)
                query = query.Take(limit);



            // *****************************************************************
            logger.Debug("Attempting to execute query");
            var list = await query.ToListAsync(ct);



            // *****************************************************************
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



    public async Task<OkOrError> Save( CancellationToken ct = default )
    {

        using var logger = EnterMethod();


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to perform evaluation");
            var eval = PerformEvaluation();
            if (eval.IsError)
                return eval.AsError;


            // *****************************************************************
            logger.Debug("Attempting to perform lifecycle callbacks");
            PerformCallbacks();



            // *****************************************************************
            logger.Debug("Attempting to perform journaling");
            var journal = PerformEntityJournaling();
            await Context.AddRangeAsync(journal, ct);



            // *****************************************************************
            logger.Debug("Attempting to save changes");
            var affected = await Context.SaveChangesAsync(ct);

            logger.Inspect(nameof(affected), affected);



            // *****************************************************************
            return Ok.Singleton;


        }
        catch (Exception cause)
        {
            logger.Error(cause, "Save failed.");

            return UnhandledError.Create(cause);

        }


    }


    public bool PerformEvaluations { get; set; }

    protected OkOrError PerformEvaluation()
    {

        using var logger = EnterMethod();

        logger.Inspect(nameof(PerformEvaluations), PerformEvaluations);

        if (!PerformEvaluations)
            return Ok.Singleton;



        // *****************************************************************
        logger.Debug("Attempting to find all dirty entities");
        var entries = Context.ChangeTracker.Entries().Where(e => Helpers.DirtyStates.Contains(e.State)).ToList();
        var entities = entries.Select(d => d.Entity).ToList();

        logger.Inspect(nameof(entities.Count), entities.Count);



        // *****************************************************************
        logger.Debug("Attempting to evaluate all tracked entities");            
        if (!rules.TryValidate( entities, out var violations ) )
            return NotValidError.Create(violations);



        // *****************************************************************
        return Ok.Singleton;


    }


    public bool PerformLifecycleCallbacks { get; set; } = true;

    protected void PerformCallbacks()
    {

        using var logger = EnterMethod();

        logger.Inspect(nameof(PerformLifecycleCallbacks), PerformLifecycleCallbacks);

        if (!PerformLifecycleCallbacks)
            return;


        foreach (var candidate in Context.ChangeTracker.Entries().Where(e=>Helpers.DirtyStates.Contains(e.State)) )
        {

            if (candidate is { State: EntityState.Added, Entity: IEntity created })
            {
                created.OnCreate();
                created.OnModification();
            }

            if (candidate is { State: EntityState.Modified, Entity: IEntity modified })
                modified.OnModification();

        }


    }


    public bool PerformJournaling { get; set; } = true;
    public IRootEntity? Root { get; set; }

    protected IList<AuditJournal> PerformEntityJournaling()
    {

        using var logger = EnterMethod();



        var journalTime = DateTime.Now;
        var journals = new List<AuditJournal>();



        // *****************************************************************
        logger.Inspect(nameof(PerformJournaling), PerformJournaling);

        if (!PerformJournaling)
            return journals;


        if (Root != null)
        {

            logger.Debug("Attempting to create unmodified root journal entry");
            var aj = CreateAuditJournal(journalTime, AuditJournalType.UnmodifiedRoot, Root);

            journals.Add(aj);
        }


        // *****************************************************************
        logger.Debug("Attempting to inspect each entity in this DbContext");
        foreach (var entry in Context.ChangeTracker.Entries() )
        {

            logger.Inspect("EntityType", entry.Entity.GetType().FullName);
            logger.Inspect("State", entry.State);


            // *****************************************************************
            logger.Debug("Attempting to check if entity is Model");
            if (entry.Entity is not IEntity entity)
                continue;



            // *****************************************************************
            logger.Debug("Attempting to get AuditAttribute");
            var audit = entry.Entity.GetType().GetCustomAttribute<AuditAttribute>(true);

            if( logger.IsDebugEnabled && audit is not null )
            {
                logger.LogObject(nameof(audit), new { audit.EntityName, audit.Read, audit.Write, audit.Detailed } );
            }

            if (audit == null || audit is { Read: false, Write: false })
                continue;



            // *****************************************************************                    
            if (audit.Read)
            {

                logger.Debug("Attempting to create read journal entry");
                var aj = CreateAuditJournal(journalTime, AuditJournalType.Read, entity);
                journals.Add(aj);

            }



            // *****************************************************************
            if (entry.State == EntityState.Added && audit.Write)
            {

                logger.Debug("Attempting to create insert journal entry");
                var aj = CreateAuditJournal(journalTime, AuditJournalType.Created, entity);

                journals.Add(aj);

                if (audit.Detailed)
                    PerformDetailJournaling(entry, journals, journalTime);

            }



            // *****************************************************************
            if (entry.State == EntityState.Modified && audit.Write)
            {

                logger.Debug("Attempting to create update journal entry");
                var aj = CreateAuditJournal(journalTime, AuditJournalType.Updated, entity);

                journals.Add(aj);

                if (audit.Detailed)
                    PerformDetailJournaling(entry, journals, journalTime);

            }



            // *****************************************************************
            if (entry.State == EntityState.Deleted && audit.Write)
            {

                logger.Debug("Attempting to create delete journal entry");
                var aj = CreateAuditJournal(journalTime, AuditJournalType.Deleted, entity);

                journals.Add(aj);

            }



            // *****************************************************************
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (entry.State == EntityState.Unchanged && audit.Write && entity is IRootEntity && Root == null)
            {

                logger.Debug("Attempting to create unmodified root journal entry");
                var aj = CreateAuditJournal(journalTime, AuditJournalType.UnmodifiedRoot, entity);

                journals.Add(aj);

            }


        }



        // *****************************************************************        
        logger.Inspect("Journal Count", journals.Count);
        return journals;



    }

    protected void PerformDetailJournaling(EntityEntry entry, IList<AuditJournal> journals, DateTime journalTime)
    {

        using var logger = EnterMethod();



        if (entry.Entity is not IEntity entity)
            return;



        foreach (var prop in entry.Properties)
        {

            if (!prop.IsModified)
                continue;

            var aj = CreateAuditJournal(journalTime, AuditJournalType.Detail, entity, prop);

            journals.Add(aj);

        }


    }



    protected AuditJournal CreateAuditJournal(DateTime journalTime, AuditJournalType type, IEntity entity, PropertyEntry? prop = null)
    {

        var ident = Correlation.ToIdentity();

        var aj = new AuditJournal
        {
            TypeCode = type.ToString(),
            UnitOfWorkUid = Correlation.Uid,
            SubjectUid = ident.GetSubject("Anonymous"),
            SubjectDescription = ident.GetName(),
            Occurred = journalTime,
            Entity = entity.GetType().FullName ?? "",
            EntityUid = entity.Uid,
            EntityDescription = entity.ToString() ?? ""
        };


        if (prop != null)
        {

            aj.PropertyName = prop.Metadata.Name;

            var prev = prop.OriginalValue?.ToString() ?? "";
            if (prev.Length > 255)
                prev = prev[..255];

            aj.PreviousValue = prev;

            var curr = prop.CurrentValue?.ToString() ?? "";
            if (curr.Length > 255)
                curr = curr[..255];

            aj.CurrentValue = curr;

        }


        return aj;

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
    Task<ListOrError<TEntity>> Many<TEntity>(Expression<Func<TEntity, bool>> predicate, int limit, CancellationToken ct = default) where TEntity : class, IEntity;


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

    public async Task<ListOrError<TEntity>> Many<TEntity>(Expression<Func<TEntity, bool>> predicate, int limit, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        try
        {

            var list = await Context.Set<TEntity>().Where(predicate).Take(limit).ToListAsync(ct);

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








