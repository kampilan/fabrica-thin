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

using System.Drawing;
using Fabrica.Watch.Sink;
using Fabrica.Watch.Utilities;

namespace Fabrica.Watch;


public class Logger : ILogger
{

    internal Logger(Action<Logger> onDispose)
    {
        OnDispose = onDispose;
    }

    private Action<Logger> OnDispose { get; }


    public void Dispose()
    {

        if( !string.IsNullOrWhiteSpace(CurrentScope) )
        {
            var le = CreateEvent( Level.Debug, CurrentScope );
            le.Nesting = -1;

            CurrentScope = "";

            LogEvent(le);
        }

        OnDispose(this);

    }

    internal void Config( IWatchFactory factory, string tenant, string subject, string tag, string category, string correlationId, Level level, Color color )
    {

        Factory = factory;

        Category = category;
        CorrelationId = correlationId;

        Tenant = tenant;
        Subject = subject;
        Tag     = tag;

        Level = level;
        Color = color;

    }


    public virtual bool IsTraceEnabled => Level == Level.Trace;

    public virtual bool IsDebugEnabled => Level <= Level.Debug;

    public virtual bool IsInfoEnabled => Level <= Level.Info;

    public virtual bool IsWarningEnabled => Level <= Level.Warning;

    public virtual bool IsErrorEnabled => Level <= Level.Error;



    internal IWatchFactory Factory { get; set; } = null!;


    public string Category { get; protected set; } = string.Empty;

    internal string Tenant { get; set; } = string.Empty;
    internal string Subject { get; set; } = string.Empty;
    internal string Tag { get; set; } = string.Empty;

    internal string CorrelationId { get; set; } = string.Empty;

    internal Level Level { get; set; }
    internal Color Color { get; set; }

    private string GetCorrelationId()
    {

        if( string.IsNullOrWhiteSpace(CorrelationId) )
            CorrelationId = Ulid.NewUlid();

        return CorrelationId;

    }

    public virtual LogEvent CreateEvent( Level level, object? title )
    {


        var le = new LogEvent();

        le.Tenant = Tenant;
        le.Subject = Subject;
        le.Tag = Tag;
        le.Category = Category;
        le.CorrelationId = GetCorrelationId();
        le.Level = (int)level;
        le.Color = Color.ToArgb();
        le.Title = title?.ToString() ?? string.Empty;

        return le;

    }

    public virtual LogEvent CreateEvent( Level level, object? title, PayloadType type, string? content )
    {


        var le = Factory.AcquireLogEvent();

        le.Tenant = Tenant;
        le.Subject = Subject;
        le.Tag = Tag;
        le.Category = Category;
        le.CorrelationId = GetCorrelationId();
        le.Level = (int)level;
        le.Color = Color.ToArgb();
        le.Title = title?.ToString() ?? string.Empty;


        if ( string.IsNullOrWhiteSpace(content) )
            return le;
       
        le.Type   = (int)type;
        le.Payload = content;

        return le;

    }


    public virtual LogEvent CreateEvent(Level level, object? title, object? obj )
    {

        var le = Factory.AcquireLogEvent();

        le.Tenant = Tenant;
        le.Subject = Subject;
        le.Tag = Tag;
        le.Category = Category;
        le.CorrelationId = GetCorrelationId();
        le.Level = (int)level;
        le.Color = Color.ToArgb();
        le.Title = title?.ToString() ?? string.Empty;

        if ( obj is null )
            return le;

        le.Object = obj;

        return le;

    }


    public virtual LogEvent CreateEvent(Level level, object? title, Exception ex,  object? context )
    {


        var le = Factory.AcquireLogEvent();

        le.Tenant = Tenant;
        le.Subject = Subject;
        le.Tag = Tag;
        le.Category = Category;
        le.CorrelationId = GetCorrelationId();
        le.Level = (int)level;
        le.Color = Color.ToArgb();
        le.Title = title?.ToString() ?? string.Empty;
        le.Error = ex;
        le.ErrorContext = context;

        return le;

    }


    public virtual void LogEvent( LogEvent logEvent )
    {

        Factory.Accept( logEvent );

    }


    private string CurrentScope { get; set; } = string.Empty;
    public string GetCurrentScope() => CurrentScope;
    public void SetCurrentScope(string name)
    {
        CurrentScope = name;
    }

    public virtual IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }



}