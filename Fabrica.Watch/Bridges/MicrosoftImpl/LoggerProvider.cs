using Microsoft.Extensions.Logging;

namespace Fabrica.Watch.Bridges.MicrosoftImpl;

public class LoggerProvider: ILoggerProvider
{


    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {

        var internalLogger = WatchFactoryLocator.Factory.GetLogger(categoryName);
        var logger = new Logger( internalLogger, categoryName );
        return logger;

    }

    public Microsoft.Extensions.Logging.ILogger CreateLogger( ILoggingCorrelation correlation, string categoryName )
    {

        var request = new LoggerRequest
        {
            CorrelationId = correlation.CorrelationId,
            Tenant = correlation.Tenant,
            Subject = correlation.Subject,
            Category = categoryName
        };
        
        var internalLogger = WatchFactoryLocator.Factory.GetLogger( ref request );
        var logger = new Logger( internalLogger, categoryName );
        return logger;

    }    

    public void Dispose()
    {
    }


}