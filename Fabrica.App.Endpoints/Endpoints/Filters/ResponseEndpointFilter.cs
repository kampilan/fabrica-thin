using System.Net;
using System.Text.Json;
using Fabrica.Exceptions;
using Fabrica.Models;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;

// ReSharper disable UnusedMember.Global

namespace Fabrica.App.Endpoints.Filters;

// ReSharper disable once UnusedType.Global
public class ResponseEndpointFilter(ICorrelation correlation, JsonSerializerOptions options): CorrelatedObject(correlation), IEndpointFilter
{

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {

        var result = await next(context);
        
        // ***********************************************************************
        // *** Result Filter starts here ***
        // ***********************************************************************        
        using var logger = EnterMethod();


        
        // ***********************************************************************        
        if (result is null)
        {
            logger.Debug( "Null Result encountered");
            return Results.NoContent();       
        }

        
        // ***********************************************************************
        switch (logger.IsTraceEnabled)
        {
            case true when result is IResult r:
            {
                var ctx = new { Type = r.GetType().GetConciseFullName() };
                logger.LogObject(nameof(IResult), ctx);
                break;
            }
            case true when result is IValueResponse vr:
            {
                var ctx = new
                {
                    Type = nameof(IValueResponse),
                    vr.IsSuccessful,
                    vr.Kind,
                };
                logger.LogObject("Response", ctx);
                break;
            }
            case true when result is IResponse ur:
            {
                var ctx = new
                {
                    Type = nameof(IResponse),
                    ur.IsSuccessful,
                    ur.Kind,
                };
                logger.LogObject("Response", ctx);
                break;
            }
            case true:
            {
                var ctx = new { Type = result.GetType().GetConciseFullName() };
                logger.LogObject(nameof(result), ctx);
                break;
            }
        }
        
        

        // *************************************************
        logger.Debug("Attempting to map Response to Result");

        switch (result)
        {
            case IResult:
                return result;
            case IValueResponse { IsSuccessful: true, Value: Stream st }:
                return Results.Stream(st, "application/json");
            case IValueResponse { IsSuccessful: true } ok:
                return Results.Json(ok.Value, options: options);
            case IResponse { IsSuccessful: true }:
                return Results.Ok();
            case IValueResponse { IsSuccessful: false } er:
            {
                var status = MapToStatus(er.Kind);

                var problemDetail = new ProblemDetail
                {
                    Type          = er.Kind.ToString(),
                    Title         = er.ErrorCode,
                    Detail        = er.Explanation,
                    StatusCode    = status,
                    Instance      = context.HttpContext.Request.Path,
                    CorrelationId = Correlation.Uid,
                    Segments      = er.Details
                };

                logger.LogObject(nameof(problemDetail), problemDetail);

                return Results.Json(problemDetail, options, "application/problem+json", status);
            }
            case IResponse { IsSuccessful: false } er2:
            {
                var status = MapToStatus(er2.Kind);

                var problemDetail = new ProblemDetail
                {
                    Type          = er2.Kind.ToString(),
                    Title         = er2.ErrorCode,
                    Detail        = er2.Explanation,
                    StatusCode    = status,
                    Instance      = context.HttpContext.Request.Path,
                    CorrelationId = Correlation.Uid,
                    Segments      = er2.Details
                };

                logger.LogObject(nameof(problemDetail), problemDetail);

                return Results.Json(problemDetail, options, "application/problem+json", status);
            }
            default:
                return Results.Json(result, options);

            
        }
        
    }


    protected virtual int MapToStatus(ErrorKind kind)
    {


        var statusCode = HttpStatusCode.InternalServerError;

        switch (kind)
        {

            case ErrorKind.None:
                statusCode = HttpStatusCode.OK;
                break;

            case ErrorKind.NotFound:
                statusCode = HttpStatusCode.NotFound;
                break;

            case ErrorKind.NotImplemented:
                statusCode = HttpStatusCode.NotImplemented;
                break;

            case ErrorKind.Predicate:
                statusCode = HttpStatusCode.UnprocessableEntity;
                break;

            case ErrorKind.Conflict:
                statusCode = HttpStatusCode.Conflict;
                break;

            case ErrorKind.Functional:
                statusCode = HttpStatusCode.InternalServerError;
                break;

            case ErrorKind.Concurrency:
                statusCode = HttpStatusCode.Gone;
                break;

            case ErrorKind.BadRequest:
                statusCode = HttpStatusCode.BadRequest;
                break;

            case ErrorKind.AuthenticationRequired:
                statusCode = HttpStatusCode.Unauthorized;
                break;

            case ErrorKind.NotAuthorized:
                statusCode = HttpStatusCode.Forbidden;
                break;

            case ErrorKind.System:
            case ErrorKind.Unknown:
                statusCode = HttpStatusCode.InternalServerError;
                break;

        }


        return (int)statusCode;

    }




}