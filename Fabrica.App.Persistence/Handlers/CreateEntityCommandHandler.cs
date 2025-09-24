using Fabrica.App.Requests;
using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Watch;

// ReSharper disable MemberCanBePrivate.Global

namespace Fabrica.App.Handlers;

public abstract class CreateEntityCommandHandler<TRequest,TEntity,TDelta>(ICommandService service) : BaseOneCommandHandler<TRequest,TEntity>(service) where TRequest: BaseOneRequest<TEntity,TDelta> where TEntity : class, IEntity, new() where TDelta : BaseDelta
{

    private static readonly string Name = typeof(TEntity).Name;
    
    public override async Task<Response<TEntity>> Handle(TRequest request, CancellationToken ct = default)
    {

        using var logger = EnterMethod();

        

        // *****************************************************************
        logger.DebugFormat( "Attempting to map Delta to new {0}", Name );
        var entity = new TEntity();
        Service.Mapper.Map(request.Delta, entity);

        logger.LogObject("Created", entity);



        // **********************************************************
        logger.DebugFormat( "Attempting to persist new {0}", Name );        
        var persistResult = await Service.Repository.PersistAsync(entity, ct);
        if( persistResult.IsError )
            return persistResult.AsError;            


        // *****************************************************************
        logger.Debug("Attempting to save changes");
        var saveResult = await Service.Repository.SaveAsync(ct);
        if (saveResult.IsError)
            return saveResult.AsError;

        
        
        // *************************************************
        logger.Debug("Attempting to call OnSuccess");
        var result = await OnSuccess( request, entity, ct );    


        
        // **********************************************************
        return result;            
        
        
    }
    
    
}