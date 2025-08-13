using Fabrica.Utilities.Container;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using Fabrica.Exceptions;
using Fabrica.Persistence.Repository;
using Fabrica.Watch;
using ILogger = Fabrica.Watch.ILogger;


namespace Fabrica.Persistence.Ef.Contexts;

public abstract class AbstractDbContext: DbContext
{


    protected AbstractDbContext( OriginDbContextOptionsBuilder builder ) : base(builder.Options)
    {

        Correlation = builder.Correlation;
        Factory     = builder.LoggerFactory;

        IsReadonly = false;

    }


    protected AbstractDbContext( ReplicaDbContextOptionsBuilder builder ) : base(builder.Options)
    {

        Correlation = builder.Correlation;
        Factory     = builder.LoggerFactory;

        IsReadonly = true;

    }


    protected bool IsReadonly { get; }

    protected ICorrelation Correlation { get; }

    private ILoggerFactory Factory { get; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseLoggerFactory(Factory);

    }


    // ReSharper disable once UnusedMember.Global
    protected ILogger GetLogger()
    {

        var logger = Correlation.GetLogger(this);

        return logger;

    }

    protected ILogger EnterMethod([CallerMemberName] string name = "")
    {

        var logger = Correlation.EnterMethod(GetType(), name);

        return logger;

    }


    private void ReadonlyCheck()
    {
        if( IsReadonly )
            throw new InvalidOperationException("This is a read only DbContext. No changes are allowed");
    }


    public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
    {
        ReadonlyCheck();
        return base.Add(entity);
    }

    public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = new ())
    {
        ReadonlyCheck();
        return base.AddAsync(entity, cancellationToken);
    }

    public override EntityEntry Add(object entity)
    {
        ReadonlyCheck();
        return base.Add(entity);
    }

    public override ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = new CancellationToken())
    {
        ReadonlyCheck();
        return base.AddAsync(entity, cancellationToken);
    }


    public override void AddRange(params object[] entities)
    {
        ReadonlyCheck();
        base.AddRange(entities);
    }

    public override void AddRange(IEnumerable<object> entities)
    {
        ReadonlyCheck();
        base.AddRange(entities);
    }

    public override Task AddRangeAsync(params object[] entities)
    {
        ReadonlyCheck();
        return base.AddRangeAsync(entities);
    }

    public override Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = new CancellationToken())
    {
        ReadonlyCheck();
        return base.AddRangeAsync(entities, cancellationToken);
    }

    public override EntityEntry<TEntity> Attach<TEntity>(TEntity entity)
    {
        ReadonlyCheck();
        return base.Attach(entity);
    }

    public override EntityEntry Attach(object entity)
    {
        ReadonlyCheck();
        return base.Attach(entity);
    }

    public override void AttachRange(params object[] entities)
    {
        ReadonlyCheck();
        base.AttachRange(entities);
    }

    public override void AttachRange(IEnumerable<object> entities)
    {
        ReadonlyCheck();
        base.AttachRange(entities);
    }

    public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
    {
        ReadonlyCheck();
        return base.Remove(entity);
    }

    public override void RemoveRange(params object[] entities)
    {
        ReadonlyCheck();
        base.RemoveRange(entities);
    }

    public override EntityEntry Remove(object entity)
    {
        ReadonlyCheck();
        return base.Remove(entity);
    }

    public override void RemoveRange(IEnumerable<object> entities)
    {
        ReadonlyCheck();
        base.RemoveRange(entities);
    }

    public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
    {
        ReadonlyCheck();
        return base.Update(entity);
    }

    public override EntityEntry Update(object entity)
    {
        ReadonlyCheck();
        return base.Update(entity);
    }

    public override void UpdateRange(params object[] entities)
    {
        ReadonlyCheck();
        base.UpdateRange(entities);
    }

    public override void UpdateRange(IEnumerable<object> entities)
    {
        ReadonlyCheck();
        base.UpdateRange(entities);
    }


    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {

        using var logger = EnterMethod();

        if( IsReadonly )
            throw new InvalidOperationException("This is a read only DbContext. No changes are allowed");

        var result = Task.Run(async () => await SaveChangesAsync(acceptAllChangesOnSuccess)).ConfigureAwait(false).GetAwaiter().GetResult();

        return result;

    }


    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();


        if( IsReadonly )
            throw new InvalidOperationException("Can not save changes on this DbContext as it is Readonly");

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);    

    }

 
    
    
    
    
    


}