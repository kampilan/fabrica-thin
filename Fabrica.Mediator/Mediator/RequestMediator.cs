using Autofac;
using Fabrica.Exceptions;
using Fabrica.Models;
using Fabrica.Rules;
using MediatR;

// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

namespace Fabrica.Mediator;

public interface IRequestMediator
{

    Task<Response<TResponse>> Send<TRequest,TResponse>( TRequest request, CancellationToken cancellationToken = new()) where TRequest: class, IRequest<Response<TResponse>> where TResponse : class;

    Task<Response> Send<TRequest>( TRequest request, CancellationToken cancellationToken = new()) where TRequest: class, IRequest<Response>;

}

internal class RequestMediator(ILifetimeScope root, IRuleSet rules): IRequestMediator
{

    protected class WrapperServiceProvider(ILifetimeScope scope) : IServiceProvider
    {
        private ILifetimeScope Scope { get; } = scope;

        public object? GetService(Type serviceType)
        {
            return Scope.ResolveOptional(serviceType);
        }

    }


    protected virtual bool TryValidateRequest<TRequest,TResponse>( TRequest request, out Response<TResponse>? error ) where TRequest: class, IRequest<Response<TResponse>> where TResponse: class
    {

        var reqType = request.GetType();

        var details = new List<EventDetail>();

        if (!rules.TryValidate(request, out var violations))
            details.AddRange(violations);

        if ( details.Count != 0 )
        {
            error = Response<TResponse>.BadRequest($"{reqType.Name} is not valid. See details below.", details);
            return false;
        }

        error = null;

        return true;


    }

    protected virtual bool TryValidateRequest( IRequest<Response> request, out Response? error)
    {

        var reqType = request.GetType();

        var details = new List<EventDetail>();
        if (!rules.TryValidate(request, out var violations))
            details.AddRange(violations);

        if (details.Count != 0)
        {
            error = Response.BadRequest($"{reqType.Name} is not valid. See details below.", details);
            return false;
        }

        error = null;

        return true;

    }


    public async Task<Response<TResponse>> Send<TRequest,TResponse>( TRequest request, CancellationToken cancellationToken = new ()) where TRequest: class, IRequest<Response<TResponse>> where TResponse: class
    {

        if( !TryValidateRequest<TRequest,TResponse>(request, out var error) && error is not null )
            return error;

        await using var scope = root.BeginLifetimeScope();

        var provider = new WrapperServiceProvider(scope);
        var inner = new MediatR.Mediator(provider);

        var response = await inner.Send(request, cancellationToken);


        return response;

    }

    public async Task<Response> Send<TRequest>( TRequest request, CancellationToken cancellationToken = new ()) where TRequest: class, IRequest<Response>
    {

        if (!TryValidateRequest(request, out var error) && error is not null)
            return error;

        await using var scope = root.BeginLifetimeScope();

        var provider = new WrapperServiceProvider(scope);
        var inner = new MediatR.Mediator(provider);

        var response = await inner.Send(request, cancellationToken);

        return response;

    }



}