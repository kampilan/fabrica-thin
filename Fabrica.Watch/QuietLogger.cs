using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;

namespace Fabrica.Watch;


public class QuietLoggerFactory: IWatchFactory
{

    public ISwitchSource Switches { get; } = new SwitchSource();

    public Task Start()
    {
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }

    public void Accept(LogEvent logEvent)
    {
    }

    public ILogger GetLogger(string category, bool retroOn = true)
    {
        return QuietLogger.Single;
    }

    public ILogger GetLogger<T>(bool retroOn = true)
    {
        return QuietLogger.Single;
    }

    public ILogger GetLogger(Type type, bool retroOn = true)
    {
        return QuietLogger.Single;
    }

    public ILogger GetLogger(ref LoggerRequest request, bool retroOn = true)
    {
        return QuietLogger.Single;
    }

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

    public LogEvent CreateEvent( Level level, object? title )
    {
        return QuietEvent;
    }

    public LogEvent CreateEvent( Level level, object? title, PayloadType type, string? payload )
    {
        return QuietEvent;
    }

    public LogEvent CreateEvent( Level level, object? title, object? payload )
    {
        return QuietEvent;
    }

    public LogEvent CreateEvent( Level level, object? title, Exception ex, object? context )
    {
        return QuietEvent;
    }

    public void LogEvent( LogEvent logEvent )
    {
    }

    public string GetCurrentScope()
    {
        return string.Empty;
    }

    public void SetCurrentScope(string name)
    {
    }


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