using Fabrica.Watch.Sink;

namespace Fabrica.Watch;

public class QuietLogger: ILogger
{

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