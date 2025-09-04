
// ReSharper disable UnusedMember.Global

using Microsoft.Extensions.Logging;

namespace Fabrica.Watch.Bridges.MicrosoftImpl;

public class LoggerFactory(ILoggingCorrelation? correlation=null): ILoggerFactory
{
    private LoggerProvider Provider { get; } = new();

    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        if (correlation is not null)
            return Provider.CreateLogger(correlation, categoryName);
        
        return Provider.CreateLogger(categoryName);
    }

    public void AddProvider( ILoggerProvider provider )
    {

    }

    public void Dispose()
    {
    }

}