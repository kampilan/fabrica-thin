﻿/*
The MIT License (MIT)

Copyright (c) 2024 Pond Hawk Technologies Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

// ReSharper disable UnusedMember.Global

using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.App.Endpoints.Middleware;

public static class MiddlewareExtensions
{


    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }

    public static IApplicationBuilder UseDiagnosticsMonitor(this IApplicationBuilder app )
    {
        app.UseMiddleware<DiagnosticsMonitorMiddleware>();
        return app;
    }

    public static IApplicationBuilder UseDiagnosticsEnrichment(this IApplicationBuilder app)
    {
        app.UseMiddleware<DiagnosticsEnrichmentMiddleware>();
        return app;
    }


    public static IServiceCollection AddDiagnosticMiddleware(this IServiceCollection collection, DiagnosticOptions? options = null)
    {

        collection.AddTransient(p =>
        {

            var corr = p.GetRequiredService<ICorrelation>();
            options ??= new DiagnosticOptions();

            var mw = new DiagnosticsMonitorMiddleware(corr, options);

            return mw;

        });


        collection.AddTransient(p =>
        {

            var corr = p.GetRequiredService<ICorrelation>();
            options ??= new DiagnosticOptions();

            var mw = new DiagnosticsEnrichmentMiddleware(corr, options);

            return mw;

        });


        return collection;


    }


}