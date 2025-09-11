using Fabrica.App.Requests;
using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Watch;

namespace Fabrica.App.Handlers;

public abstract class UpdateEntityCommandHandler<TRequest,TEntity,TDelta>(ICommandService service, Func<IQueryable<TEntity>,IQueryable<TEntity>>? eager=null ): BaseOneCommandHandler<TRequest,TEntity>(service) where TRequest: BaseOneRequest<TEntity,TDelta> where TEntity : class, IEntity, new() where TDelta : BaseDelta
{

    private static readonly string Name = typeof(TEntity).Name;

    protected virtual IQueryable<TEntity> GetEagerQueryable( IQueryable<TEntity> set ) => eager is null ? set: eager(set);    
    
    public override async Task<Response<TEntity>> Handle(TRequest request, CancellationToken ct = default)
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.DebugFormat("Attempting to fetch {0} by Uid", Name );
        var entity = await Service.Repository.EagerByUidAsync<TEntity>( request.Uid, GetEagerQueryable, ct );
        if( entity is null )
            return Response<TEntity>.NotFound($"Could not find {Name} using Uid: ({request.Uid})");


        
        // *****************************************************************
        logger.DebugFormat( "Attempting to map Delta to new {0}", Name );
        Service.Mapper.Map(request.Delta, entity);

        logger.LogObject("Updated", entity);



        // *****************************************************************
        logger.Debug("Attempting to save changes");
        await Service.Repository.SaveAsync(ct);

        Service.Uow.CanCommit();


        
        // *************************************************
        logger.Debug("Attempting to call OnSuccess");
        var result = await OnSuccess( request, entity, ct );    
        
       
        
        // *****************************************************************
        return entity;            
        
        
    }    

    
}