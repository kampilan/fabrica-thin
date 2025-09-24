using System.Data.Common;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fabrica.App.Persistence.Interceptors;

public class SqlLoggingInterceptor(ICorrelation correlation): DbCommandInterceptor
{

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {

        using var logger = correlation.GetLogger("App.Persistence.SQL");
        
        logger.LogSql("SQL", command.CommandText);
        
        return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {

        using var logger = correlation.GetLogger("App.Persistence.SQL");
        
        logger.LogSql("SQL", command.CommandText);
        
        return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        
    }
    
}