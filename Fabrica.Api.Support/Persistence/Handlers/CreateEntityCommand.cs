using Fabrica.Api.Persistence.Requests;
using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using MediatR;

namespace Fabrica.Api.Persistence.Handlers;

public abstract class CreateEntityCommand<TRequest, TEntity, TDelta>( ICommandService service ) : CorrelatedObject(service.Correlation), IRequestHandler<TRequest, Response> where TRequest : BaseCreateEntityRequest<TDelta> where TEntity : class, IEntity, new() where TDelta : BaseDelta
{

    protected ICommandService Service { get; init; } = service;

    protected virtual Task<TEntity> CreateEntity()
    {
        var entity = new TEntity();
        return Task.FromResult(entity);
    }

    protected virtual Task Before(TRequest request, TEntity entity)
    {
        return Task.CompletedTask;
    }

    protected virtual Task After(TRequest request, TEntity entity)
    {
        return Task.CompletedTask;
    }


    public async Task<Response> Handle(TRequest request, CancellationToken token = default)
    {

        using var logger = EnterMethod();


        var entityTypeName = typeof(TEntity).GetConciseFullName();
        logger.Inspect(nameof(entityTypeName), entityTypeName);

        var deltaTypeName = typeof(TDelta).GetConciseFullName();
        logger.Inspect(nameof(deltaTypeName), deltaTypeName);


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to create new entity");
            var entity = await CreateEntity();



            // *****************************************************************
            logger.Debug("Attempting to call before");
            await Before(request, entity);



            // *****************************************************************
            logger.Debug("Attempting to map delta to entity");
            Service.Mapper.Map(request.Delta, entity);



            // *****************************************************************
            logger.Debug("Attempting to persist entity");
            Service.DbContext.Add(entity);



            // *****************************************************************
            logger.Debug("Attempting to call after");
            await After(request, entity);



            // *****************************************************************
            logger.Debug("Attempting to save changes");
            var affected = await Service.DbContext.SaveChangesAsync(token);



            // *****************************************************************
            logger.Debug("Attempting to mark Uow as CanCommit");
            Service.Uow.CanCommit();



            // *****************************************************************
            return Response.Ok(entity.Uid, 1);


        }
        catch (Exception)
        {
            Service.Uow.MustRollback();
            throw;
        }


    }


}