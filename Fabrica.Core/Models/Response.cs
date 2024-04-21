using Fabrica.Exceptions;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Models;


public interface IResponse
{

    bool IsSuccessful { get; }

    ErrorKind Kind { get; }
    string ErrorCode { get; }
    string Explanation { get; }

    List<EventDetail> Details { get; }

}



public class Response: IResponse
{


    public static implicit operator Response( Error error )
    {

        var res = new Response
        {
            _successful = false,
            Kind        = error.Kind,
            ErrorCode   = error.ErrorCode,
            Explanation = error.Explanation,
            Details     = [.. error.Details]
        };

        return res;

    }


    private bool _successful;
    public bool IsSuccessful => _successful;

    public void Success() => _successful = true;

    public void Failure() => _successful = false;


    public ErrorKind Kind { get; protected set; } = ErrorKind.System;
    public string ErrorCode { get; protected set; } = string.Empty;
    public string Explanation { get; protected set; } = string.Empty;

    public List<EventDetail> Details { get; protected set; } = [];


    public static Response Ok()
    {
        var response = new Response();
        response.Success();
        return response;
    }


    public static Response NotFound(string explanation)
    {

        var response = new Response();

        response.Failure();

        response.Kind = ErrorKind.NotFound;
        response.ErrorCode = "Resource not found";
        response.Explanation = explanation;

        return response;

    }


    public static Response BadRequest(string explanation)
    {

        var response = new Response();

        response.Failure();

        response.Kind = ErrorKind.BadRequest;
        response.ErrorCode = "Could not process request";
        response.Explanation = explanation;

        return response;

    }



    public static Response BadRequest(string explanation, IEnumerable<EventDetail> violations)
    {

        var response = new Response();

        response.Failure();

        response.Kind = ErrorKind.BadRequest;
        response.ErrorCode = "Validation errors exist";
        response.Explanation = explanation;

        response.Details.AddRange(violations);

        return response;

    }


    public static Response FailedValidation(string explanation, IEnumerable<EventDetail> violations)
    {

        var response = new Response();

        response.Failure();

        response.Kind = ErrorKind.Predicate;
        response.ErrorCode = "Validation errors exist";
        response.Explanation = explanation;

        response.Details.AddRange(violations);

        return response;

    }

    public static Response Failed(ErrorKind kind, string errorCode, string explanation, IEnumerable<EventDetail>? details = null)
    {

        var response = new Response();

        response.Failure();

        response.Kind = kind;
        response.ErrorCode = errorCode;
        response.Explanation = explanation;

        if (details is not null)
            response.Details.AddRange(details);

        return response;

    }

    public static Response Failed( IResponse source )
    {

        var response = new Response();

        response.Failure();

        response.Kind = source.Kind;
        response.ErrorCode = source.ErrorCode;
        response.Explanation = source.Explanation;

        response.Details.AddRange(source.Details);

        return response;

    }


}

public interface IValueResponse
{
    
    bool IsSuccessful { get; }
    object Value { get; }

    ErrorKind Kind { get;  }
    string ErrorCode { get; }
    string Explanation { get; }

    List<EventDetail> Details { get; }

}

public class Response<TResponse> : IValueResponse where TResponse : class
{



    public static implicit operator Response<TResponse>(TResponse value)
    {

        var res = new Response<TResponse>
        {
            _value = value,
            _successful = true
        };

        return res;

    }

    public static implicit operator Response<TResponse>( Error error )
    {

        var res = new Response<TResponse>
        {
            _successful = false,
            ErrorCode   = error.ErrorCode,
            Explanation = error.Explanation,
            Details     = [..error.Details]
        };

        return res;

    }


    object IValueResponse.Value => Value;


    private bool _successful;
    public bool IsSuccessful => _successful;

    public void Success( TResponse value )
    {
        _value = value;
        _successful = true;
    }

    public void Failure() => _successful = false;


    private TResponse _value = null!;
    public TResponse Value => _value;


    public ErrorKind Kind { get; protected set; } = ErrorKind.System;
    public string ErrorCode { get; protected set; } = string.Empty;
    public string Explanation { get; protected set; } = string.Empty;

    public List<EventDetail> Details { get; protected set; } = [];


    public static Response<TResponse> Ok(TResponse value)
    {
        var response = new Response<TResponse>();
        response.Success(value);
        return response;
    }


    public static Response<TResponse> NotFound(string explanation)
    {

        var response = new Response<TResponse>();

        response.Failure();

        response.Kind = ErrorKind.NotFound;
        response.ErrorCode = "Resource not found";
        response.Explanation = explanation;

        return response;

    }


    public static Response<TResponse> BadRequest(string explanation)
    {

        var response = new Response<TResponse>();

        response.Failure();

        response.Kind = ErrorKind.BadRequest;
        response.ErrorCode = "Could not process request";
        response.Explanation = explanation;

        return response;

    }

    public static Response<TResponse> BadRequest(string explanation, IEnumerable<EventDetail> violations)
    {

        var response = new Response<TResponse>();

        response.Failure();

        response.Kind = ErrorKind.BadRequest;
        response.ErrorCode = "Validation errors exist";
        response.Explanation = explanation;

        response.Details.AddRange(violations);

        return response;

    }


    public static Response<TResponse> FailedValidation(string explanation, IEnumerable<EventDetail> violations )
    {

        var response = new Response<TResponse>();

        response.Failure();

        response.Kind = ErrorKind.Predicate;
        response.ErrorCode = "Validation errors exist";
        response.Explanation = explanation;

        response.Details.AddRange(violations);

        return response;

    }


    public static Response<TResponse> Failed(ErrorKind kind, string errorCode, string explanation, IEnumerable<EventDetail>? details=null )
    {

        var response = new Response<TResponse>();

        response.Failure();

        response.Kind = kind;
        response.ErrorCode = errorCode;
        response.Explanation = explanation;

        if( details is not null )
            response.Details.AddRange(details);

        return response;

    }

    public static Response<TResponse> Failed(IValueResponse source)
    {

        var response = new Response<TResponse>();

        response.Failure();

        response.Kind = source.Kind;
        response.ErrorCode = source.ErrorCode;
        response.Explanation = source.Explanation;

        response.Details.AddRange(source.Details);

        return response;

    }


}