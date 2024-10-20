﻿using Autofac;
using Fabrica.Exceptions;
using Fabrica.Models;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;

// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

namespace Fabrica.Mediator;

public interface IRequestMediator
{

    Task<Response<TResponse>> Send<TResponse>( IRequest<Response<TResponse>> request, CancellationToken cancellationToken = new()) where TResponse : class;

    Task<Response> Send<TRequest>( TRequest request, CancellationToken cancellationToken = new()) where TRequest: class, IRequest<Response>;

    Task<Response> Send(IEnumerable<IRequest<Response>> requests, CancellationToken cancellationToken = new());


}

internal class RequestMediator(ILifetimeScope root, ICorrelation correlation, IRuleSet rules): CorrelatedObject(correlation), IRequestMediator
{

    protected class WrapperServiceProvider(ILifetimeScope scope) : IServiceProvider
    {
        private ILifetimeScope Scope { get; } = scope;

        public object? GetService(Type serviceType)
        {
            return Scope.ResolveOptional(serviceType);
        }

    }


    protected virtual bool TryValidateRequest<TResponse>(IRequest<Response<TResponse>> request, out Response<TResponse>? error ) where TResponse: class
    {

        using var logger = EnterMethod();


        var reqType = request.GetType();

        var details = new List<EventDetail>();



        // *****************************************************************
        logger.Debug("Attempting to validate the request");
        if (!rules.TryValidate(request, out var violations))
            details.AddRange(violations);

        if ( details.Count != 0 )
        {
            logger.Debug("Validation failed.");
            error = Response<TResponse>.BadRequest($"{reqType.Name} is not valid. See details below.", details);
            return false;
        }

        error = null;

        return true;


    }

    protected virtual bool TryValidateRequest( IRequest<Response> request, out Response? error)
    {

        using var logger = EnterMethod();


        var reqType = request.GetType();

        var details = new List<EventDetail>();


        // *****************************************************************
        logger.Debug("Attempting to validate the request");
        if (!rules.TryValidate(request, out var violations))
            details.AddRange(violations);

        if (details.Count != 0)
        {
            logger.Debug("Validation failed.");
            error = Response.BadRequest($"{reqType.Name} is not valid. See details below.", details);
            return false;
        }

        error = null;

        return true;

    }


    public async Task<Response<TResponse>> Send<TResponse>(IRequest<Response<TResponse>> request, CancellationToken cancellationToken = new ()) where  TResponse: class
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(request), request);


        if( !TryValidateRequest(request, out var error) && error is not null )
            return error;


        await using var scope = root.BeginLifetimeScope(cb =>
        {
            cb.CloneCorrelation(Correlation);
        });


        var provider = new WrapperServiceProvider(scope);
        var inner = new MediatR.Mediator(provider);

        var response = await inner.Send(request, cancellationToken);

        if (logger.IsDebugEnabled)
        {
            var ctx = new {response.IsSuccessful, response.ErrorCode, response.Explanation, response.Details};
            logger.LogObject(nameof(response), ctx);
        }


        return response;

    }

    public async Task<Response> Send<TRequest>( TRequest request, CancellationToken cancellationToken = new ()) where TRequest: class, IRequest<Response>
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(request), request);


        if (!TryValidateRequest(request, out var error) && error is not null)
            return error;


        await using var scope = root.BeginLifetimeScope(cb =>
        {
            cb.CloneCorrelation(Correlation);
        });

        var provider = new WrapperServiceProvider(scope);
        var inner = new MediatR.Mediator(provider);

        var response = await inner.Send(request, cancellationToken);

        if (logger.IsDebugEnabled)
        {
            var ctx = new { response.IsSuccessful, response.ErrorCode, response.Explanation, response.Details };
            logger.LogObject(nameof(response), ctx);
        }


        return response;

    }

    public async Task<Response> Send( IEnumerable<IRequest<Response>> requests, CancellationToken cancellationToken = new())
    {

        using var logger = EnterMethod();


        await using var scope = root.BeginLifetimeScope(cb =>
        {
            cb.CloneCorrelation(Correlation);
        });

        var provider = new WrapperServiceProvider(scope);
        var inner = new MediatR.Mediator(provider);

        foreach( var request in requests )
        {

            logger.LogObject(nameof(request), request);


            if (!TryValidateRequest(request, out var error) && error is not null)
                return error;


            var innerRes = await inner.Send(request, cancellationToken);

            if( logger.IsDebugEnabled )
            {
                var ctx = new { innerRes.IsSuccessful, innerRes.ErrorCode, innerRes.Explanation, innerRes.Details };
                logger.LogObject(nameof(innerRes), ctx);
            }

            if( !innerRes.IsSuccessful )
                return innerRes;

        }

        return Response.Ok();

    }




}