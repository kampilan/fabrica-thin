﻿/*
The MIT License (MIT)

Copyright (c) 2024 The Pond Hawk Technologies Inc.

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

using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using Fabrica.Identity;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Text;
using Fabrica.Watch;
using Fabrica.Watch.Sink;
using Microsoft.AspNetCore.Http;

// ReSharper disable UnusedMember.Global

namespace Fabrica.App.Endpoints.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next)
{

    private RequestDelegate Next { get; } = next;


    public async Task Invoke( HttpContext context, ICorrelation correlation )
    {

        if (context == null) throw new ArgumentNullException(nameof(context));
        if (correlation == null) throw new ArgumentNullException(nameof(correlation));


        // ****************************************************************************************
        var subject = "";
        if (correlation.Caller is ClaimsPrincipal cp)
            subject = cp.GetUserName();

        var lr = new LoggerRequest {Category = "Fabrica.Diagnostics.Http", CorrelationId = correlation.Uid, Subject = subject, Level = Level.Warning };
        var diagLogger = WatchFactoryLocator.Factory.GetLogger( ref lr );


        if( diagLogger.IsTraceEnabled )
        {
            var builder = new StringBuilder();
            await BuildRequest(context, correlation, builder);

            if (builder.Length > 0)
            {
                var le = diagLogger.CreateEvent(Level.Trace, $"Diagnostics - HTTP Request {context.Request.Method.ToUpper()} {context.Request.Path}", PayloadType.Text, builder.ToString());
                diagLogger.LogEvent(le);
            }

        }

        else if ( diagLogger.IsDebugEnabled )
        {

            var le = diagLogger.CreateEvent(Level.Debug, $"Diagnostics - HTTP Request {context.Request.Method.ToUpper()} {context.Request.Path}", PayloadType.None);
            diagLogger.LogEvent(le);

        }


        // ****************************************************************************************
        await Next(context);



        // ****************************************************************************************
        if ( diagLogger.IsDebugEnabled )
        {
            var le = diagLogger.CreateEvent(Level.Debug, $"Diagnostics - HTTP Response {context.Request.Method.ToUpper()} {context.Request.Path} - Status: {context.Response.StatusCode}", PayloadType.None );
            diagLogger.LogEvent(le);
        }


    }



    private async Task BuildRequest( HttpContext context, ICorrelation correlation, StringBuilder builder )
    {

        using var logger = correlation.EnterMethod<RequestLoggingMiddleware>();

        try
        {

            // *****************************************************************
            logger.Debug("Attempting to build host related members");

            var host  = $"{context.Request.Host}{context.Request.PathBase}";
            var route = $"{context.Request.Method.ToUpper()} {context.Request.Path}";
            var query = context.Request.QueryString.ToString();



            // *****************************************************************
            logger.Debug("Attempting to gather HTTP Headers");

            var headers = new Dictionary<string, object>();
            foreach (var (key, values) in context.Request.Headers)
            {
                string value;

                switch (key)
                {

                    case "Authorization":

                        if (values.Count <= 0)
                            continue;

                        var pos = values[0]?.IndexOf(" ", 0, StringComparison.Ordinal) ?? 0;
                        if (pos > 0)
                        {
                            var scheme = values[0]![..pos];
                            var len = values[0]!.Length - pos;
                            value = $"Scheme: {scheme} Length: {len}";
                        }
                        else
                            value = values[0] ?? "";

                        break;

                    case "X-Gateway-Identity-Token":

                        if (values.Count <= 0)
                            continue;

                        value = $"Length: {values[0]?.Length??0}";

                        break;

                    case "X-Gateway-Identity":

                        if (values.Count <= 0)
                            continue;

                        value = $"Length: {values[0]?.Length ?? 0}";

                        break;

                    case "Cookie":

                        var names = context.Request.Cookies.Keys.ToList();
                        value = string.Join(',', names);
                        break;

                    default:

                        value = string.Join(",", values.ToArray());
                        break;

                }


                headers[key] = value;

            }

            var claims = new Dictionary<string, string>();
            if (context.User.Identity?.IsAuthenticated ?? false)
            {
                foreach (var claim in context.User.Claims)
                    claims[claim.Type] = claim.Value;
            }



            // *****************************************************************
            logger.Debug("Attempting to dig out Body content");

            string bodyContent = "";


            var body = new MemoryStream();
            await context.Request.Body.CopyToAsync(body);
            body.Seek(0, SeekOrigin.Begin);

            if (context.Request.ContentType == "application/json")
            {
                var reader = new StreamReader(body);
                var json = await reader.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(json))
                    bodyContent = MakeJsonPretty(json);
            }
            else if (context.Request.ContentType == "application/xml")
            {
                var reader = new StreamReader(body);
                var xml = await reader.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(xml))
                    bodyContent = MakeXmlPretty(xml);
            }
            else if (context.Request.ContentType == "application/x-www-form-urlencoded")
            {
                var reader = new StreamReader(body);
                var form = await reader.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(form))
                    bodyContent = MakeFormPretty(form);
            }
            else
            {
                var reader = new StreamReader(body);
                bodyContent = await reader.ReadToEndAsync();
            }

            body.Seek(0, SeekOrigin.Begin);
            context.Request.Body = body;



            // *****************************************************************
            logger.Debug("Attempting to build logging payload");

            builder.AppendLine("********************************************************************************");
            builder.AppendLine("HTTP Request Details");
            builder.AppendLine("********************************************************************************");
            builder.AppendLine();

            builder.AppendLine("Request");
            builder.AppendLine("********************************************************************************");
            builder.AppendFormat(" Host: ({0})", host);
            builder.AppendLine();
            builder.AppendFormat("Route: ({0})", route);
            builder.AppendLine();
            builder.AppendFormat("Query: ({0})", query);
            builder.AppendLine();
            builder.AppendLine();


            if (headers.Count > 0)
            {

                var padding = headers.Max(p => p.Key.Length);
                builder.AppendLine("Headers");
                builder.AppendLine("********************************************************************************");
                foreach (var (key, value) in headers)
                {
                    var label = key.PadRight(padding);
                    builder.AppendFormat("{0} : {1}", label, value);
                    builder.AppendLine();

                }
                builder.AppendLine();

            }


            if ( claims.Count > 0 )
            {

                var padding = claims.Max(p => p.Key.Length);
                builder.AppendLine("Claims");
                builder.AppendLine("********************************************************************************");
                foreach (var (key, value) in claims)
                {
                    var label = key.PadRight(padding);
                    builder.AppendFormat("{0} : {1}", label, value);
                    builder.AppendLine();

                }
                builder.AppendLine();

            }


            if ( !string.IsNullOrWhiteSpace(bodyContent) )
            {
                builder.AppendLine("Body");
                builder.AppendLine("********************************************************************************");
                builder.AppendLine(bodyContent);
                builder.AppendLine();
                builder.AppendLine("********************************************************************************");
            }



        }
        catch (Exception cause)
        {
            logger.Error(cause, "An error occurred during request logging");
        }


    }
    

    protected string MakeJsonPretty( string json )
    {

        if (string.IsNullOrWhiteSpace(json))
            return "{}";

        var pretty = json;
        try
        {
            var tok = JsonNode.Parse(json);
            pretty = tok?.ToJsonString(new JsonSerializerOptions {WriteIndented = true})??"{}";
        }
        catch
        {
            // ignored
        }

        return pretty;

    }


    protected virtual string MakeXmlPretty( string xml )
    {

        var pretty = xml;
        try
        {

            var document = new XmlDocument();
            document.LoadXml(xml);

            using var writer = new StringWriter();
            using var xw = new XmlTextWriter(writer);

            xw.Formatting = Formatting.Indented;
            document.Save(xw);
            writer.Flush();
            pretty = writer.ToString();

        }
        catch
        {
            // ignored
        }

        return pretty;

    }


    protected virtual string MakeFormPretty( string form )
    {

        var values  = new UrlEncodingParser(form);
        var padding = values.AllKeys.Max(k => k?.Length??0);

        var builder = new StringBuilder();
        foreach( var key in values.AllKeys )
        {

            var label = key?.PadRight(padding)??"Unknown".PadRight(padding);
            var value = values[key];

            builder.AppendFormat("{0} : {1}", label, value );
            builder.AppendLine();

        }

        return builder.ToString();

    }




}