
// ReSharper disable UnusedMember.Global

using Microsoft.Extensions.Logging;

namespace Fabrica.Watch.Bridges.MicrosoftImpl;

public class LoggerFactory(ILoggingCorrelation correlation): ILoggerFactory
{
    private LoggerProvider Provider { get; } = new();

    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        var logger = Provider.CreateLogger(correlation, categoryName);
        return logger;
    }

    public void AddProvider( ILoggerProvider provider )
    {

    }

    public void Dispose()
    {
    }

}