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

using Fabrica.Watch.Utilities;
using System.Drawing;
using System.Runtime.CompilerServices;
using Fabrica.Watch.Switching;

namespace Fabrica.Watch.Sink;

public class ConsoleEventSink: IEventSinkProvider, ILogger
{


    public virtual Task Start()
    {
        return Task.CompletedTask;
    }

    public virtual Task Stop()
    {
        return Task.CompletedTask;
    }


    public virtual Task Accept( LogEventBatch batch, CancellationToken ct=default )
    {

        foreach ( var le in batch.Events )
            _write(le);

        return Task.CompletedTask;

    }

    public void Accept(LogEvent logEvent)
    {
        _write(logEvent);
    }

    private void _write( LogEvent le)
    {

        switch (le.Level)
        {
            case (int)Level.Trace:
            case (int)Level.Debug:
                Console.ForegroundColor = ConsoleColor.Green;
                break;

            case (int)Level.Info:
                Console.ForegroundColor = ConsoleColor.Blue;
                break;

            case (int)Level.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;

            case (int)Level.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;

            case (int)Level.Quiet:
                Console.ForegroundColor = ConsoleColor.White;
                break;
        }


        Console.WriteLine("================================================================================");

        var level = (Level)le.Level;
        var dt = WatchHelpers.FromWatchTimestamp(le.Occurred);

        var message = $"{dt:T} - {level.ToString().ToUpper()} - {le.Category} - {le.Title}";
        Console.WriteLine(message);
        if (le.Type != (int)PayloadType.None)
        {
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine(le.Payload);
        }

        Console.ResetColor();

    }

    public string Category { get; private set; } = string.Empty;


    public bool Quiet { get; set; }
    public ILogger GetLogger(string category, bool retro=false)
    {

        if(Quiet)
            return QuietLogger.Single;

        Category = category;

        return this;

    }

    public ILogger GetLogger<T>( bool retro=false )
    {

        if (Quiet)
            return QuietLogger.Single;

        Category = typeof(T).GetConciseFullName();

        return this;

    }

    public ILogger GetLogger( Type target, bool retro = false)
    {

        if (Quiet)
            return QuietLogger.Single;

        target.GetConciseFullName();

        return this;

    }



    public ILogger EnterMethod<T>( [CallerMemberName] string name = "" )
    {

        if( Quiet )
            return QuietLogger.Single;

        Category = typeof(T).GetConciseFullName();

        (this as ILogger).EnterMethod(name);

        return this;

    }




    public void Dispose()
    {

        if (!string.IsNullOrWhiteSpace(_scope))
        {
            var le = (this as ILogger).CreateEvent(Level.Debug, _scope);
            le.Nesting = -1;

            _scope = "";

            (this as ILogger).LogEvent(le);
        }

    }

    private string _correlationId = string.Empty;
    private string GetCorrelationId()
    {

        if (string.IsNullOrWhiteSpace(_correlationId))
            _correlationId = Ulid.NewUlid();

        return _correlationId;

    }

    private readonly Color _color = Color.WhiteSmoke;

    string ILogger.Category => Category;

    bool ILogger.IsTraceEnabled => !Quiet;

    bool ILogger.IsDebugEnabled => !Quiet;

    bool ILogger.IsInfoEnabled => !Quiet;

    bool ILogger.IsWarningEnabled => !Quiet;

    bool ILogger.IsErrorEnabled => !Quiet;


    private static TextExceptionSerializer ForException { get; } = new();

    LogEvent ILogger.CreateEvent(Level level, object? title)
    {

        if( Quiet )
            return LogEvent.Single;

        var le = new LogEvent();

        le.Tenant = string.Empty;
        le.Subject = string.Empty;
        le.Tag = string.Empty;
        le.Category = Category;
        le.CorrelationId = GetCorrelationId();
        le.Level = (int)level;
        le.Color = _color.ToArgb();
        le.Title = title?.ToString() ?? string.Empty;
        le.Occurred = WatchHelpers.ToWatchTimestamp();

        return le;


    }

    LogEvent ILogger.CreateEvent(Level level, object? title, PayloadType type, string? payload)
    {

        if (Quiet)
            return LogEvent.Single;

        var le = new LogEvent();

        le.Tenant = string.Empty;
        le.Subject = string.Empty;
        le.Tag = string.Empty;
        le.Category = Category;
        le.CorrelationId = GetCorrelationId();
        le.Level = (int)level;
        le.Color = _color.ToArgb();
        le.Title = title?.ToString() ?? string.Empty;
        le.Occurred = WatchHelpers.ToWatchTimestamp();

        return le;


    }

    LogEvent ILogger.CreateEvent(Level level, object? title, object? payload)
    {

        if (Quiet)
            return LogEvent.Single;

        var le = new LogEvent();

        le.Tenant = string.Empty;
        le.Subject = string.Empty;
        le.Tag = string.Empty;
        le.Category = Category;
        le.CorrelationId = GetCorrelationId();
        le.Level = (int)level;
        le.Color = _color.ToArgb();
        le.Title = title?.ToString() ?? string.Empty;
        le.Occurred = WatchHelpers.ToWatchTimestamp();

        return le;

    }

    LogEvent ILogger.CreateEvent(Level level, object? title, Exception ex, object? context)
    {

        if( Quiet )
            return LogEvent.Single;

        var le = new LogEvent();

        le.Tenant = string.Empty;
        le.Subject = string.Empty;
        le.Tag = string.Empty;
        le.Category = Category;
        le.CorrelationId = GetCorrelationId();
        le.Level = (int)level;
        le.Color = _color.ToArgb();
        le.Title = title?.ToString() ?? string.Empty;
        le.Occurred = WatchHelpers.ToWatchTimestamp();

        var (type, source) = ForException.Serialize(ex, context);
        le.Type = (int)type;
        le.Payload = source;

        return le;

    }

    void ILogger.LogEvent(LogEvent logEvent)
    {
        Accept(LogEventBatch.Single("",logEvent));
    }

    private string _scope = string.Empty;
    string ILogger.GetCurrentScope()
    {
        return _scope;
    }

    void ILogger.SetCurrentScope(string name)
    {
        _scope = name;
    }

    IDisposable ILogger.BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }


}

public class ConsoleLoggerFactory : IWatchFactory
{

    private ConsoleEventSink TheSink { get; } = new();

    public ISwitchSource Switches { get; set; } = null!;

    public IEventSinkProvider? GetSink<T>() where T : class, IEventSinkProvider
    {
        return TheSink;
    }

    public async Task Start()
    {

        var ss = new SwitchSource();
        ss.WhenNotMatched(Level.Debug, Color.Green);

        Switches = ss;
        
        await TheSink.Start();

    }

    public async Task Stop()
    {
        await TheSink.Stop();
    }

    public void Accept(LogEvent logEvent)
    {
        TheSink.Accept(logEvent);
    }

    public ILogger GetLogger(string category, bool retroOn = true)
    {
        return TheSink.GetLogger(category, retroOn);
    }

    public ILogger GetLogger<T>(bool retroOn = true)
    {
        return TheSink.GetLogger<T>( retroOn);
    }

    public ILogger GetLogger(Type type, bool retroOn = true)
    {
        return TheSink.GetLogger( type, retroOn );
    }

    public ILogger GetLogger(ref LoggerRequest request, bool retroOn = true)
    {
        return TheSink.GetLogger(request.Category, retroOn);
    }

    public LogEvent AcquireLogEvent()
    {

        var le = new LogEvent();
        le.Reset();

        return le;

    }


}