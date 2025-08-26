﻿using System.Net;
using System.Text.Json;
using Fabrica.Exceptions;
using Fabrica.Models;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Filters;

public class ResponseEndpointFilter(ICorrelation correlation, JsonSerializerOptions options): CorrelatedObject(correlation), IEndpointFilter
{

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {

        var result = await next(context);

        
        // shortcut the response process for 401 APIKey Auth failed most likely
        if( result is UnauthorizedHttpResult || context.HttpContext.Response.StatusCode == 401 )
        {
            var obj = new { CustomErrorMessage = "401 Unauthorized" };
            return  Results.Json(obj, options, "application/json", 401);           
        }

        // shortcut the response process for 403        
        if( result is ForbidHttpResult || context.HttpContext.Response.StatusCode == 403 )
        {
            var obj = new { CustomErrorMessage = "403 Forbidden" };
            return  Results.Json(obj, options, "application/json", 403);           
        }        
        
        
        using var logger = EnterMethod();

        
        if (result is IValueResponse {IsSuccessful: true} ok)
        {

            if( ok.Value is Stream st )
                return Results.Stream(st, "application/json");

            return Results.Json(ok.Value, options: options);

        }

        if (result is IResponse { IsSuccessful: true })
            return Results.Ok();


        if (result is IValueResponse { IsSuccessful: false } er)
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

        if( result is IResponse { IsSuccessful: false } er2 )
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


        return Results.Json(result, options);


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