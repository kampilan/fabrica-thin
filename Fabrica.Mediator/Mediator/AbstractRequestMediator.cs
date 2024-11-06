using Autofac;
using Fabrica.Exceptions;
using Fabrica.Models;
using Fabrica.Rules;
using Fabrica.Rules.Exceptions;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using MediatR;
using System.Runtime.CompilerServices;
using Fabrica.Rql.Parser;

// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

namespace Fabrica.Mediator;

public interface IRequestMediator
{

    void Configure( ILifetimeScope root, ICorrelation correlation, IRuleSet rules );

    Task<Response<TResponse>> Send<TResponse>( IRequest<Response<TResponse>> request, CancellationToken cancellationToken = new()) where TResponse : class;

    Task<Response> Send<TRequest>( TRequest request, CancellationToken cancellationToken = new()) where TRequest: class, IRequest<Response>;

    Task<Response> Send(CompositeRequest composite, CancellationToken cancellationToken = new());

    Task<Response<TModel>> Send<TModel>( CompositeRequest<TModel> composite, CancellationToken cancellationToken = new()) where TModel: class;


}


public class RequestMediator : AbstractRequestMediator;

public abstract class AbstractRequestMediator: IRequestMediator
{


    protected ILifetimeScope Root { get; private set; } = null!;
    protected ICorrelation Correlation { get; private set; } = null!;
    protected IRuleSet Rules { get; private set; } = null!;


    public void Configure( ILifetimeScope root, ICorrelation correlation, IRuleSet rules )
    {
        Root        = root;
        Correlation = correlation;
        Rules       = rules;
    }

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


    protected class WrapperServiceProvider( ILifetimeScope scope ) : IServiceProvider
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
        if (!Rules.TryValidate(request, out var violations))
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
        if( !Rules.TryValidate(request, out var violations) )
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


    protected virtual Task<Response<TResponse>> HandleException<TResponse>( IRequest<Response<TResponse>> request, Exception cause ) where TResponse : class
    {

        Response<TResponse> error;

        switch (cause)
        {
            
            case ViolationsExistException ve:
                error = Response<TResponse>.FailedValidation( ve.Explanation, ve.Violations );
                break;

            case ExternalException ee:
                error = Response<TResponse>.Failed(ee.Kind, ee.ErrorCode, ee.Explanation, ee.Details);
                break;

            default:

                var ctx = new { RequestType = request.GetType().GetConciseFullName(), ResponseType=typeof(TResponse).GetConciseFullName() };
                GetLogger().ErrorWithContext(cause, ctx, "Unhandled Exception");

                var ec = cause.GetType().FullName ?? "";
                error = Response<TResponse>.Failed(ErrorKind.System, ec, cause.Message);
                break;    

        }

        return Task.FromResult(error);

    }


    protected virtual Task<Response> HandleException( IRequest<Response> request,  Exception cause )
    {

        Response error;

        switch (cause)
        {

            case ViolationsExistException ve:
                error = Response.FailedValidation(ve.Explanation, ve.Violations);
                break;

            case RqlException pe:
                error = Response.Failed( ErrorKind.BadRequest, "InvalidRQL", pe.Message );
                break;

            case ExternalException ee:
                error = Response.Failed(ee.Kind, ee.ErrorCode, ee.Explanation, ee.Details);
                break;

            default:

                var ctx = new { RequestType = request.GetType().GetConciseFullName() };
                GetLogger().ErrorWithContext(cause, ctx, "Unhandled Exception");

                var ec = cause.GetType().FullName ?? "";
                error = Response.Failed(ErrorKind.System, ec, cause.Message);
                break;

        }

        return Task.FromResult(error);

    }



    public async Task<Response<TResponse>> Send<TResponse>(IRequest<Response<TResponse>> request, CancellationToken cancellationToken = new ()) where  TResponse: class
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(request), request);



        // *****************************************************************
        logger.Debug("Attempting to validate request");
        if( !TryValidateRequest(request, out var error) && error is not null )
            return error;



        // *****************************************************************
        logger.Debug("Attempting to create Lifetime scope for this send");
        await using var scope = Root.BeginLifetimeScope(cb =>
        {
            cb.CloneCorrelation(Correlation);
        });

        var provider = new WrapperServiceProvider(scope);
        var inner = new MediatR.Mediator(provider);



        // *****************************************************************
        logger.Debug("Attempting to send request via inner Mediator");
        Response<TResponse> response;
        try
        {
            response = await inner.Send(request, cancellationToken);
            logger.Debug("Send completed without Exception");
        }
        catch (Exception cause)
        {
            logger.Debug("Send completed WITH Exception");
            response = await HandleException( request,cause );
        }


        if( logger.IsDebugEnabled )
        {
            var ctx = new {response.IsSuccessful, response.ErrorCode, response.Explanation, response.Details, Value=typeof(TResponse).GetConciseFullName() };
            logger.LogObject(nameof(response), ctx);
        }



        // *****************************************************************
        return response;

    }


    public async Task<Response> Send<TRequest>( TRequest request, CancellationToken cancellationToken = new ()) where TRequest: class, IRequest<Response>
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(request), request);


        // *****************************************************************
        logger.Debug("Attempting to validate request");
        if (!TryValidateRequest(request, out var error) && error is not null)
            return error;



        // *****************************************************************
        logger.Debug("Attempting to create Lifetime scope for this send");
        await using var scope = Root.BeginLifetimeScope(cb =>
        {
            cb.CloneCorrelation(Correlation);
        });

        var provider = new WrapperServiceProvider(scope);
        var inner = new MediatR.Mediator(provider);



        // *****************************************************************
        logger.Debug("Attempting to send request via inner Mediator");
        Response response;
        try
        {
            response = await inner.Send(request, cancellationToken);
            logger.Debug("Send completed without Exception");
        }
        catch (Exception cause)
        {
            logger.Debug("Send completed WITH Exception");
            response = await HandleException( request, cause );
        }


        if( logger.IsDebugEnabled )
        {
            var ctx = new { response.IsSuccessful, response.ErrorCode, response.Explanation, response.Details, response.Affected };
            logger.LogObject(nameof(response), ctx);
        }



        // *****************************************************************
        return response;


    }

    public async Task<Response> Send( CompositeRequest composite, CancellationToken cancellationToken = new())
    {

        using var logger = EnterMethod();


        await using var scope = Root.BeginLifetimeScope(cb =>
        {
            cb.CloneCorrelation(Correlation);
        });

        var provider = new WrapperServiceProvider(scope);
        var inner = new MediatR.Mediator(provider);

        foreach( var request in composite.Components )
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



    public async Task<Response<TModel>> Send<TModel>(CompositeRequest<TModel> composite, CancellationToken cancellationToken = new()) where TModel: class
    {

        using var logger = EnterMethod();


        await using var scope = Root.BeginLifetimeScope(cb =>
        {
            cb.CloneCorrelation(Correlation);
        });

        var provider = new WrapperServiceProvider(scope);
        var inner = new MediatR.Mediator(provider);

        foreach (var request in composite.Components)
        {

            logger.LogObject(nameof(request), request);


            if (!TryValidateRequest(request, out var error) && error is not null)
                return Response<TModel>.Failed(error.Kind, error.ErrorCode, error.Explanation, error.Details);


            var innerRes = await inner.Send(request, cancellationToken);

            if (logger.IsDebugEnabled)
            {
                var ctx = new { innerRes.IsSuccessful, innerRes.ErrorCode, innerRes.Explanation, innerRes.Details };
                logger.LogObject(nameof(innerRes), ctx);
            }

            if (!innerRes.IsSuccessful)
                return Response<TModel>.Failed(innerRes.Kind, innerRes.ErrorCode, innerRes.Explanation, innerRes.Details);

        }


        {

            var request = composite.Responder;

            logger.LogObject(nameof(request), request);


            if (!TryValidateRequest(request, out var error) && error is not null)
                return Response<TModel>.Failed(error.Kind, error.ErrorCode, error.Explanation, error.Details);


            var innerRes = await inner.Send(request, cancellationToken);

            if( logger.IsDebugEnabled )
            {
                var ctx = new { innerRes.IsSuccessful, innerRes.ErrorCode, innerRes.Explanation, innerRes.Details };
                logger.LogObject(nameof(innerRes), ctx);
            }

            if( !innerRes.IsSuccessful )
                return Response<TModel>.Failed(innerRes.Kind, innerRes.ErrorCode, innerRes.Explanation, innerRes.Details);

            return innerRes;


        }



    }




}