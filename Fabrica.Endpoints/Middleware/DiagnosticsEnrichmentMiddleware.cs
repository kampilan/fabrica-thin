using Fabrica.Identity.Gateway;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Middleware;

public class DiagnosticsEnrichmentMiddleware( ICorrelation correlation, DiagnosticOptions options) : IMiddleware
{

    public DiagnosticsEnrichmentMiddleware(ICorrelation correlation) : this(correlation, new DiagnosticOptions())
    {
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {

        using var logger = correlation.EnterMethod<DiagnosticsEnrichmentMiddleware>();


        var token = context.Request.Headers[IdentityConstants.TokenHeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(token))
            logger.Debug("Does not have Gateway Token");

        var caller = context.User;


        // *****************************************************************
        logger.Debug("Attempting to set Caller on Correlation");
        if (correlation is Correlation impl)
        {

            impl.CallerGatewayToken = token ?? "";
            impl.Caller = caller;

            options.Enrich(impl);

        }

        
        // ****************************************************************************************
        await next(context);


    }


}