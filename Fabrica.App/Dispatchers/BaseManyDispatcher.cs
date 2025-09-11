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
/// Represents a base class to handle Many resource requests with dispatching functionality.
/// This abstract class provides a structure for building the request and sending it through a mediator while logging the process.
/// </summary>
/// <typeparam name="TRequest">
/// The type of the request object. It must be a class implementing <see cref="IRequest{Response{List{TResponse}}}"/>.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of the response object. It must be a class.
/// </typeparam>
public abstract class BaseManyDispatcher<TRequest, TResponse> where TRequest : class, IRequest<Response<List<TResponse>>> where TResponse : class
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

    public async Task<Response<List<TResponse>>> Send()
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


/// <summary>
/// Represents a base class for dispatching Many requests  and mapping the response to DTOs.
/// This abstract class provides functionalities including building requests, sending them through a mediator, and logging the process, while supporting DTO mapping.
/// </summary>
/// <typeparam name="TRequest">
/// The type of the request object. It must be a class implementing <see cref="IRequest{Response{List{TResponse}}}"/>.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of the response resource. It must be a class.
/// </typeparam>
/// <typeparam name="TDto">
/// The type of the Data Transfer Object (DTO) to which the response should be mapped. It must be a class.
/// </typeparam>
public abstract class BaseManyDispatcher<TRequest,TResponse,TDto> where TRequest : class, IRequest<Response<List<TResponse>>> where TResponse : class where TDto: class
{

    [FromServices]
    public IRequestMediator Mediator { get; set; } = null!;

    [FromServices]
    public IMapper Mapper { get; set; } = null!;


    protected abstract TRequest Build();

    public async Task<Response<List<TDto>>> Send()
    {

        using var logger = this.EnterMethod();

        
        // *************************************************
        logger.Debug("Attempting to build request");
        var request = Build();

        
        // *************************************************
        logger.Debug("Attempting to send to Mediator");
        var response = await Mediator.Send(request);



        if (!response.IsSuccessful)
            return Response<List<TDto>>.Failed(response);


        // *************************************************
        logger.Debug("Attempting to map to Dto");
        var list = Mapper.Map<List<TDto>>(response.Value);


        // *************************************************        
        return Response<List<TDto>>.Ok(list);

        
    }


}

