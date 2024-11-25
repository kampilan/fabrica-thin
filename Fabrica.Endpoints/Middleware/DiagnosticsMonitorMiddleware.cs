/*
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

using System.Diagnostics;
using System.Drawing;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Middleware;


public class DiagnosticOptions
{

    public string HeaderName { get; set; } = "X-Diagnostics-Probe";
    public Level Level { get; set; } = Level.Debug;

    public Action<Correlation> Enrich { get; set; } = _ => { };


}


public class DiagnosticsMonitorMiddleware( ICorrelation correlation, DiagnosticOptions options ) : IMiddleware
{

    public DiagnosticsMonitorMiddleware( ICorrelation correlation ) : this(correlation, new DiagnosticOptions())
    {
    }

    private ICorrelation Correlation { get; } = correlation;
    private DiagnosticOptions Options { get; } = options;

    public async Task InvokeAsync( HttpContext context, RequestDelegate next )
    {
            
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (next == null) throw new ArgumentNullException(nameof(next));

        var sw = new Stopwatch();
        sw.Start();

        using ( var logger = Correlation.GetLogger(this) )
        {

            var debug = false;

            // *****************************************************************
            logger.Debug("Attempting to check for Fabrica-Watch-Debug header");
            if (context.Request.Headers.TryGetValue(Options.HeaderName, out var header))
            {

                var df = header.FirstOrDefault();

                logger.DebugFormat("{0} IS present", Options.HeaderName);

                logger.Inspect(nameof(df), df);


                logger.Debug("Attempting to check candidate is a valid int");
                if (int.TryParse(df, out var debugFlag))
                    debug = debugFlag != 0;
                else
                    logger.Debug("Not a valid value");

            }
            else
            {
                logger.DebugFormat("{0} IS NOT present", Options.HeaderName);
            }


            if( Correlation is Correlation impl && debug )
            {
                impl.Debug = true;
                impl.Level = Options.Level;
            }


        }


        // ****************************************************************************************
        var lr = new LoggerRequest { Category = "Fabrica.Diagnostics.Http", CorrelationId = Correlation.Uid, Level = Level.Warning };
        var diagLogger = WatchFactoryLocator.Factory.GetLogger( ref lr );

        diagLogger.Debug("Diagnostics - Begin Correlation" );


        await next(context);



        // ****************************************************************************************
        sw.Stop();
        diagLogger.DebugFormat("Diagnostics - End Correlation Duration: {0} millisecond(s)", sw.ElapsedMilliseconds);



    }


}