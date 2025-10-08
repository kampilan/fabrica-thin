using System.Runtime.CompilerServices;
using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;

namespace Fabrica.Watch;


public class QuietLoggerFactory: IWatchFactory
{

    public ISwitchSource Switches { get; } = new SwitchSource();

    public Task StartAsync()
    {
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public void Accept(LogEvent logEvent)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public Task FlushEventsAsync(TimeSpan waitInterval = default, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public Task UpdateSwitchesAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public bool IsTraceEnabled(string category)
    {
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public bool IsDebugEnabled(string category)
    {
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public bool IsTraceEnabled(Type type)
    {
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public bool IsDebugEnabled(Type type)
    {
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public bool IsTraceEnabled<T>()
    {
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public bool IsDebugEnabled<T>()
    {
        return false;
    }

    public ILogger GetConsoleLogger<T>() where T : class
    {
        return QuietLogger.Single;   
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public ILogger GetLogger(string category, bool retroOn = true)
    {
        return QuietLogger.Single;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public ILogger GetLogger<T>(bool retroOn = true)
    {
        return QuietLogger.Single;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public ILogger GetLogger(Type type, bool retroOn = true)
    {
        return QuietLogger.Single;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public ILogger GetLogger(ref LoggerRequest request, bool retroOn = true)
    {
        return QuietLogger.Single;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public LogEvent AcquireLogEvent()
    {
        return LogEvent.Single;
    }
    
}


public class QuietLogger: ILogger
{

    public static ILogger Single { get; } = new QuietLogger();

    private static LogEvent QuietEvent { get; } =  new LogEvent { Level = (int)Level.Quiet };

    public void AddToRetro(string message)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LogEvent CreateEvent( Level level, object? title )
    {
        return QuietEvent;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public LogEvent CreateEvent( Level level, object? title, PayloadType type, string? payload )
    {
        return QuietEvent;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public LogEvent CreateEvent( Level level, object? title, object? payload )
    {
        return QuietEvent;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public LogEvent CreateEvent( Level level, object? title, Exception ex, object? context )
    {
        return QuietEvent;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public void LogEvent( LogEvent logEvent )
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public string GetCurrentScope()
    {
        return string.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public void SetCurrentScope(string name)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public IDisposable BeginScope<TState>(TState state)
    {
        return this;
    }


    public string Category => "";

    public bool IsTraceEnabled => false;
    public bool IsDebugEnabled => false;
    public bool IsInfoEnabled => false;
    public bool IsWarningEnabled => false;
    public bool IsErrorEnabled => false;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }


}