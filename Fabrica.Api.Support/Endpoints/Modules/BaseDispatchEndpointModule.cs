using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Humanizer;
using MediatR;

namespace Fabrica.Api.Endpoints.Modules;


public abstract class BaseDispatchEndpointModule<TRequest,TEntity>(ICorrelation correlation, IRequestMediator mediator) : CorrelatedObject(correlation) where TRequest : class, IRequest<Response> where TEntity : class, IEntity
{

    protected string Name => typeof(TEntity).Name;
    protected string Plural => Name.Pluralize();
    protected string Lower => Plural.ToLowerInvariant();

    protected virtual string GetResourceName()
    {
        return Lower;
    }

    protected virtual string GetRoute()
    {
        var resource = GetResourceName();
        return $"/{resource}";
    }

    protected async Task<Response> Handle( TRequest request )
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to send request to Mediator");
        var response = await mediator.Send(request);


        // *****************************************************************
        return response;

    }


}


public abstract class BaseDeltaDispatchEndpointModule<TRequest,TEntity>(ICorrelation correlation, IRequestMediator mediator) : CorrelatedObject(correlation) where TRequest : class, IRequest<Response> where TEntity : class, IEntity
{

    protected string Name => typeof(TEntity).Name;
    protected string Plural => Name.Pluralize();
    protected string Lower => Plural.ToLowerInvariant();

    protected virtual string GetResourceName()
    {
        return Lower;
    }

    protected virtual string GetRoute()
    {
        var resource = GetResourceName();
        return $"/{resource}";
    }

    protected async Task<Response> Handle(TRequest request)
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to send request to Mediator");
        var response = await mediator.Send(request);


        // *****************************************************************
        return response;

    }


}



public abstract class BaseOneDispatchEndpointModule<TRequest, TEntity>(ICorrelation correlation, IRequestMediator mediator) : CorrelatedObject(correlation) where TRequest : class, IRequest<Response<TEntity>> where TEntity : class, IEntity
{

    protected string Name => typeof(TEntity).Name;
    protected string Plural => Name.Pluralize();
    protected string Lower => Plural.ToLowerInvariant();

    protected virtual string GetResourceName()
    {
        return Lower;
    }

    protected virtual string GetRoute()
    {
        var resource = GetResourceName();
        return $"/{resource}";
    }

    protected async Task<Response<TEntity>> Handle( TRequest request )
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to send request to Mediator");
        var response = await mediator.Send(request);


        // *****************************************************************
        return response;

    }


}


public abstract class BaseManyDispatchEndpointModule<TRequest,TEntity>(ICorrelation correlation, IRequestMediator mediator): CorrelatedObject(correlation) where TRequest : class, IRequest<Response<IEnumerable<TEntity>>> where TEntity : class, IEntity
{

    protected string Name => typeof(TEntity).Name;
    protected string Plural => Name.Pluralize();
    protected string Lower => Plural.ToLowerInvariant();

    protected virtual string GetResourceName()
    {
        return Lower;
    }

    protected virtual string GetRoute()
    {
        var resource = GetResourceName();
        return $"/{resource}";
    }

    protected async Task<Response<IEnumerable<TEntity>>> Handle( TRequest request )
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to send request to Mediator");
        var response = await mediator.Send(request);


        // *****************************************************************
        return response;

    }


}