using Fabrica.Api.Persistence.Requests;
using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Api.Persistence.Handlers;

public abstract class DeleteEntityCommand<TRequest, TEntity>(ICommandService service) : CorrelatedObject(service.Correlation), IRequestHandler<TRequest, Response> where TRequest : BaseDeleteEntityRequest<TEntity> where TEntity : class, IEntity, new()
{

    protected ICommandService Service { get; init; } = service;


    protected virtual Task Before(TRequest request, TEntity entity)
    {
        return Task.CompletedTask;
    }

    protected virtual Task After(TRequest request, TEntity entity)
    {
        return Task.CompletedTask;
    }


    public async Task<Response> Handle(TRequest request, CancellationToken cancellationToken)
    {

        using var logger = EnterMethod();

        var entityTypeName = typeof(TEntity).GetConciseFullName();
        logger.Inspect(nameof(entityTypeName), entityTypeName);


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to create new entity");
            var entity = await Service.DbContext.Set<TEntity>().SingleOrDefaultAsync(e => e.Uid == request.Uid, cancellationToken);
            if (entity is null)
                return Response.NotFound($"Could not find {entityTypeName} using Uid ({request.Uid})");



            // *****************************************************************
            logger.Debug("Attempting to delete entity");
            Service.DbContext.Set<TEntity>().Remove(entity);



            // *****************************************************************
            logger.Debug("Attempting to save changes");
            var affected = await Service.DbContext.SaveChangesAsync(cancellationToken);



            // *****************************************************************
            return Response.Ok(request.Uid, affected);


        }
        catch (Exception)
        {
            Service.Uow.MustRollback();
            throw;
        }



    }

}