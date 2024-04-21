using System.Net;
using System.Text.Json;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Exceptions;

public class ExceptionHandler( ICorrelation correlation, JsonSerializerOptions options ): IExceptionHandler
{


    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {

        using var logger = this.EnterMethod();


        var instance = $"{httpContext.Request.Path}";
        var status = MapExceptionToStatus(exception);
        var error = BuildResponseModel(instance, status, exception);


        if( status == 500 )
            logger.ErrorWithContext( exception, error,  "An unhandled Exception  was caught." );
        else
            logger.Debug( exception, "An unhandled Exception was caught.");


        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(error, options, cancellationToken);

        return true;

    }


    protected virtual int MapExceptionToStatus(Exception exception)
    {

        if (exception == null) throw new ArgumentNullException(nameof(exception));


        var kind = ErrorKind.System;

        if (exception is ExternalException externalException)
            kind = externalException.Kind;
        else if (exception is JsonException)
            kind = ErrorKind.BadRequest;


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


    protected virtual ProblemDetail BuildResponseModel(string instance, int statusCode, Exception exception)
    {

        if (exception == null) throw new ArgumentNullException(nameof(exception));


        // ***********************************************************************
        if (exception is JsonException je)
        {
            var errorRes = new ProblemDetail
            {
                Type = "BadJsonRequest",
                Title = "Invalid JSON in Request",
                StatusCode = statusCode,
                Detail = $"Bad JSON in request near {je.Path} Line {je.LineNumber} Column {je.BytePositionInLine}",
                Instance = instance,
                CorrelationId = correlation.Uid
            };

            return errorRes;

        }

        // ***********************************************************************
        if (exception is ExternalException bex)
        {

            var errorRes = new ProblemDetail
            {
                Type = bex.ErrorCode,
                Title = "Error encountered",
                StatusCode = statusCode,
                Detail = bex.Explanation,
                Instance = instance,
                CorrelationId = correlation.Uid,
                Segments = bex.Details
            };



            return errorRes;

        }


        var defErrorRes = new ProblemDetail
        {
            Type = "InternalError",
            Title = "Internal Error encountered",
            StatusCode = statusCode,
            Detail = "An unhandled exception was encountered. Examine logs for details",
            Instance = instance,
            CorrelationId = correlation.Uid
        };


        return defErrorRes;


    }




}