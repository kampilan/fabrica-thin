using System.Runtime.CompilerServices;
using Fabrica.App.Mediator;
using Fabrica.Models;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Fabrica.App.Dispatchers;

/// <summary>
/// Represents a base class for handling dispatch logic for requests implementing <see cref="IRequest{Response}"/>.
/// </summary>
/// <typeparam name="TRequest">
/// The type of the request object, which must implement <see cref="IRequest{Response}"/>.
/// </typeparam>
public abstract class BaseDispatcher<TRequest> where TRequest : class, IRequest<Response>
{

    public HttpContext Context { get; set; } = null!;

    [FromServices]    
    public ICorrelation Correlation { get; set; } = null!;

    [FromServices] 
    public IRequestMediator Mediator { get; set; } = null!;


    protected ILogger GetLogger()
    {

        var logger = Correlation.GetLogger(this);

        return logger;

    }

    protected ILogger EnterMethod( [CallerMemberName] string name = "" )
    {

        var logger = Correlation.EnterMethod(GetType(), name);

        return logger;

    }

    protected abstract TRequest Build();

    public async Task<Response> Send()
    {

        using var logger = this.EnterMethod();

        
        // *************************************************
        logger.Debug("Attempting to build request");
        var request = Build();

        
        // *************************************************
        logger.Debug("Attempting to send to Mediator");
        var response = await Mediator.Send(request);

        
        // *************************************************        
        return response;

    }


}