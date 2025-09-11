using System.Runtime.CompilerServices;
using Fabrica.Models;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IRequestMediator = Fabrica.App.Mediator.IRequestMediator;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Fabrica.App.Dispatchers;


/// <summary>
/// Represents a base dispatcher capable of handling a One request and response cycle
/// for a specified request and response type. This class facilitates the implementation
/// of an execution flow that utilizes IRequestMediator to handle requests and responses in
/// a consistent manner.
/// </summary>
/// <typeparam name="TRequest">
/// The type of the request object implementing IRequest<Response<TResponse>>.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of the response object returned by the request.
/// </typeparam>
public abstract class BaseOneDispatcher<TRequest, TResponse>() where TRequest : class, IRequest<Response<TResponse>> where TResponse : class
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

    public async Task<Response<TResponse>> Send()
    {

        using var logger = EnterMethod();

        
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


/// <summary>
/// Provides a base implementation for dispatching a One request-response cycle that supports
/// mapping the response to a specified Data Transfer Object (DTO). This class enables the
/// execution of request-response handling in conjunction with an IRequestMediator and Mapster
/// IMapper to manage the request, raw response and its conversion to a DTO.
/// </summary>
/// <typeparam name="TRequest">
/// The type of the request object that implements IRequest<Response<TResponse>>.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of the raw response object returned by the request.
/// </typeparam>
/// <typeparam name="TDto">
/// The type of the mapped Data Transfer Object returned from the raw response.
/// </typeparam>
public abstract class BaseOneDispatcher<TRequest,TResponse,TDto> where TRequest : class, IRequest<Response<TResponse>> where TResponse : class where TDto: class
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

    [FromServices]
    public IMapper Mapper { get; set; } = null!;


    protected abstract TRequest Build();

    public async Task<Response<TDto>> Send()
    {

        using var logger = EnterMethod();

        
        // *************************************************
        logger.Debug("Attempting to build request");
        var request = Build();

        
        // *************************************************
        logger.Debug("Attempting to send to Mediator");
        var response = await Mediator.Send(request);

        
        // *************************************************
        logger.Debug("Attempting to map to Dto");
        var dto = Mapper.Map<TDto>(response.Value);

        
        // *************************************************        
        return Response<TDto>.Ok(dto);

    }


}

