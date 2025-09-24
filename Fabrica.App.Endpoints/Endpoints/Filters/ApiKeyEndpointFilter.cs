
// ReSharper disable UnusedMember.Global

using System.Security.Cryptography;
using System.Text;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;

namespace Fabrica.App.Endpoints.Filters;

public interface IApiKeyValidator
{

    bool IsValid(string candidate);

}


public abstract class AbstractApiKeyValidator(ICorrelation correlation) : CorrelatedObject(correlation), IApiKeyValidator
{

    protected abstract string GetApiKey();

    public bool IsValid(string candidate)
    {

        var apiKey = GetApiKey();

        var bufA = Encoding.ASCII.GetBytes(apiKey);
        var spanA = new ReadOnlySpan<byte>(bufA);

        var bufB = Encoding.ASCII.GetBytes(candidate);
        var spanB = new ReadOnlySpan<byte>(bufB);

        var matched = CryptographicOperations.FixedTimeEquals(spanA, spanB);


        return matched;


    }


}


public class SimpleApiKeyValidator( ICorrelation correlation, string apiKey ) : AbstractApiKeyValidator(correlation)
{

    protected override string GetApiKey()
    {
        return apiKey;
    }

}



public class ApiKeyEndpointFilter( IApiKeyValidator validator, ICorrelation correlation ) : CorrelatedObject(correlation), IEndpointFilter
{


    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {

        using var logger = EnterMethod();

        var header = context.HttpContext.Request.Headers["x-api-key"];
        var key = header.FirstOrDefault();

        if( string.IsNullOrWhiteSpace(key) )
        {
            logger.Warning("API key not present");
            return Results.Unauthorized();
        }
        
        if( !validator.IsValid(key) )
        {
            logger.Warning("API key not Valid");
            return Results.Unauthorized();
        }

        return await next(context);


    }


}
