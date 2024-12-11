using System.Data.Common;
using System.Diagnostics;
using Fabrica.Watch.Sink;
using Microsoft.Data.Sqlite;
using TaskSchedulerEngine;

namespace Fabrica.Watch.SqlIte;

public class SqliteSink: IEventSinkProvider
{

    public string Domain { get; set; } = string.Empty;

    public string ConnectionString { get; set; } = string.Empty;

    public bool UseWriteAheadLogJournaling { get; set; }

    public bool QuietLogging { get; set; } = true;

    public string CleanupCron { get; set; } = "";

    public TimeSpan DebugTimeToLive { get; set; } = TimeSpan.FromHours(4);
    public TimeSpan NonDebugTimeToLive { get; set; } = TimeSpan.FromDays(7);

    private TaskEvaluationRuntime? Runtime { get; set; }

    private SemaphoreSlim Lock { get; } = new (1, 1);

    private ConsoleEventSink DebugSink { get; } = new();


    public async Task Start()
    {

        DebugSink.Quiet = QuietLogging;


        using var logger = DebugSink.EnterMethod<SqliteSink>();

        // *****************************************************************
        logger.Debug("Attempting to validate Configuration ");
        try
        {

            if (string.IsNullOrWhiteSpace(Domain))
                throw new InvalidOperationException("Domain can not be null or blank");

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException("ConnectionString can not be null or blank");

        }
        catch (Exception cause)
        {
            logger.Error( cause, "Configuration is invalid" );
        }


        // *****************************************************************
        logger.Debug("Attempting to check DDL and create tables as necessary");
        try
        {

            await using var cn = new SqliteConnection(ConnectionString);
            await cn.OpenAsync();

            if( UseWriteAheadLogJournaling )
            {
                var pragma = cn.CreateCommand();
                pragma.CommandText = "PRAGMA journal_mode=WAL";
                await pragma.ExecuteNonQueryAsync();
            }


            await using var trx = await cn.BeginTransactionAsync();

            var cmd = cn.CreateCommand();
            cmd.CommandText = LogEventDdl;

            await cmd.ExecuteNonQueryAsync();
            await trx.CommitAsync();


        }
        catch (Exception cause)
        {
            logger.Error(cause, "Failed to execute DDL");
        }



        // *****************************************************************
        logger.Debug("Attempting to check of CleanUp task is being scheduled");            
        if( !string.IsNullOrWhiteSpace(CleanupCron) )
        {

            logger.Inspect(nameof(CleanupCron), CleanupCron);

            try
            {

                Runtime = new TaskEvaluationRuntime();
                Runtime.CreateSchedule()
                    .FromCron(CleanupCron)
                    .Execute(PerformCleanup);

#pragma warning disable CS4014
                Runtime.RunAsync();
#pragma warning restore CS4014


            }
            catch (Exception cause)
            {
                logger.Error(cause, "Failed to configure and start scheduler");
            }

        }


    }


    public async Task Stop()
    {

        using var logger = DebugSink.EnterMethod<SqliteSink>();


        // *****************************************************************
        logger.Debug("Attempting to stop schedule if it was started");
        try
        {


            if( Runtime is not null )    
                await Runtime.StopAsync();


        }
        catch (Exception cause)
        {
            logger.Error(cause, "Failed to configure and start scheduler");
        }


    }

    public async Task Accept( Stream source )
    {

        using var logger = DebugSink.EnterMethod<SqliteSink>();

        LogEventBatch? batch = null;
        try
        {
            batch = await LogEventBatchSerializer.FromStream(source);
        }
        catch (Exception cause)
        {
            logger.Error( cause, "Failed to deserialize LogEventBatch from Stream");
        }


        if (batch is null)
            return;

        try
        {

            await Accept(batch);

        }
        catch (Exception cause)
        {
            logger.Error(cause, "Failed to process LogEventBatch from Stream");
        }


    }


    public async Task Accept( LogEventBatch batch, CancellationToken ct = default)
    {

        using var logger = DebugSink.EnterMethod<SqliteSink>();

        try
        {

            await Lock.WaitAsync(ct);

            var start = Stopwatch.GetTimestamp();

            await using var cn = new SqliteConnection(ConnectionString);
            await cn.OpenAsync(ct);

            await using var transaction = await cn.BeginTransactionAsync(ct);

            var cmd = cn.CreateCommand();
            cmd.CommandText = LogEventInsertDml;

            Add(cmd, "Domain");
            Add(cmd, nameof(LogEvent.CorrelationId));
            Add(cmd, nameof(LogEvent.Category));
            Add(cmd, nameof(LogEvent.Tenant));
            Add(cmd, nameof(LogEvent.Subject));
            Add(cmd, nameof(LogEvent.Tag));
            Add(cmd, nameof(LogEvent.Level));
            Add(cmd, nameof(LogEvent.Color));
            Add(cmd, nameof(LogEvent.Nesting));
            Add(cmd, nameof(LogEvent.Title));
            Add(cmd, nameof(LogEvent.Type));
            Add(cmd, nameof(LogEvent.Payload));
            Add(cmd, nameof(LogEvent.Occurred));
            Add(cmd, "TimeToLive");


            foreach (var le in batch.Events)
            {

                var ttl = le.Level <= (int)Level.Debug ? le.Occurred + DebugTimeToLive : le.Occurred + NonDebugTimeToLive;

                Set(cmd, "Domain", Domain);
                Set(cmd, nameof(LogEvent.CorrelationId), le.CorrelationId);
                Set(cmd, nameof(LogEvent.Category), le.Category);
                Set(cmd, nameof(LogEvent.Tenant), le.Tenant);
                Set(cmd, nameof(LogEvent.Subject), le.Subject);
                Set(cmd, nameof(LogEvent.Tag), le.Tag);
                Set(cmd, nameof(LogEvent.Level), le.Level);
                Set(cmd, nameof(LogEvent.Color), le.Color);
                Set(cmd, nameof(LogEvent.Nesting), le.Nesting);
                Set(cmd, nameof(LogEvent.Title), le.Title);
                Set(cmd, nameof(LogEvent.Type), le.Type);
                Set(cmd, nameof(LogEvent.Payload), le.Payload ?? "");
                Set(cmd, nameof(LogEvent.Occurred), le.Occurred);
                Set(cmd, "TimeToLive", ttl);

                await cmd.ExecuteNonQueryAsync(ct);

            }

            await transaction.CommitAsync(ct);

            var dur = Stopwatch.GetTimestamp() - start;
            var ts = TimeSpan.FromTicks(dur);
            logger.DebugFormat("Persisted {0} events in {1} ms", batch.Events.Count, ts.TotalMilliseconds);


        }
        catch (Exception cause)
        {
            logger.Error(cause, "Failed persisting log events");
        }
        finally
        {
            Lock.Release();
        }


    }

    public async Task<List<CorrelationGroup>> GetCorrelationGroups( string domain, DateTime begin, DateTime end, CancellationToken ct=default )
    {

        using var logger = DebugSink.EnterMethod<SqliteSink>();
    
        var start = Stopwatch.GetTimestamp();

        var groups = new List<CorrelationGroup>();
        try
        {
            await using var cn = new SqliteConnection(ConnectionString);
            await cn.OpenAsync(ct);

            var cmd = cn.CreateCommand();
            cmd.CommandText = LogEventsByRangeDml;

            Add(cmd, "Domain", domain );
            Add(cmd, "Begin", ToTimestamp(begin));
            Add(cmd, "End", ToTimestamp(end));
            
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while( await reader.ReadAsync(ct) )
            {
                var group = new CorrelationGroup
                {
                    CorrelationId = reader.GetString(0),
                    Subject       = reader.GetString(1),
                    Level         = reader.GetInt32(2),
                    Begin         = FromTimestamp(reader.GetInt64(3)),
                    End           = FromTimestamp(reader.GetInt64(4)),
                };

                groups.Add(group);

            }

            var dur = Stopwatch.GetTimestamp() - start;
            var ts = TimeSpan.FromTicks(dur);
            logger.DebugFormat("Retrieved {0} Correlation Groups in {1} ms", groups.Count, ts.TotalMilliseconds);


        }
        catch (Exception cause)
        {
            logger.Error(cause, "Failed to retrieve Correlation Groups");
            throw;
        }

        return groups;


    }


    public async Task<List<Packet>> GetEventsByCorrelation(string correlationId, CancellationToken ct=default)
    {

        using var logger = DebugSink.EnterMethod<SqliteSink>();

        var start = Stopwatch.GetTimestamp();

        var packets = new List<Packet>();
        try
        {
            await using var cn = new SqliteConnection(ConnectionString);
            await cn.OpenAsync(ct);

            var cmd = cn.CreateCommand();
            cmd.CommandText = LogEventsByCorrelationDml;

            Add(cmd, "CorrelationId", correlationId);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var packet = new Packet
                {
                    CorrelationId = reader.GetString(0),
                    Category = reader.GetString(1),
                    Tenant = reader.GetString(2),
                    Subject = reader.GetString(3),
                    Tag = reader.GetString(4),
                    Level = reader.GetInt32(5),
                    Color = reader.GetInt32(6),
                    Nesting = reader.GetInt32(7),
                    Title = reader.GetString(8),
                    Type = reader.GetInt32(9),
                    Payload = reader.GetString(10),
                    Occurred = FromTimestamp(reader.GetInt64(11))
                };

                packets.Add(packet);

            }

            var dur = Stopwatch.GetTimestamp() - start;
            var ts = TimeSpan.FromTicks(dur);
            logger.DebugFormat("Retrieved {0} Packets in {1} ms", packets.Count, ts.TotalMilliseconds);


        }
        catch (Exception cause)
        {
            logger.Error(cause, "Failed to retrieve Packets");
            throw;
        }

        return packets;

    }

    private static DateTime Epoch { get; } = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    private static long ToTimestamp(DateTime target = default)
    {

        if (target == default)
            target = DateTime.Now;

        var ts = (long)(target.ToUniversalTime() - Epoch).TotalMicroseconds;

        return ts;

    }

    public static DateTime FromTimestamp(long ts) => Epoch.AddMicroseconds(ts).ToLocalTime();


    private static void Add( DbCommand command, string name )
    {

        var parameter = command.CreateParameter();

        parameter.ParameterName = $"${name}";
    
        command.Parameters.Add(parameter);

    }

    private static void Add(DbCommand command, string name, object value )
    {

        var parameter = command.CreateParameter();

        parameter.ParameterName = $"${name}";
        parameter.Value = value;

        command.Parameters.Add(parameter);


    }

    private static void Set(DbCommand command, string name, object value)
    {
        command.Parameters[$"${name}"].Value = value;
    }


    private async Task<bool> PerformCleanup(ScheduleRuleMatchEventArgs args, CancellationToken ct)
    {

        using var logger = DebugSink.EnterMethod<SqliteSink>();

        var start = Stopwatch.GetTimestamp();

        try
        {

            await Lock.WaitAsync(ct);


            await using var cn = new SqliteConnection(ConnectionString);
            await cn.OpenAsync(ct);

            await using var trx = await cn.BeginTransactionAsync(ct);

            var cmd = cn.CreateCommand();
            cmd.CommandText = LogEventCleanupDml;

            var p = cmd.CreateParameter();

            p.ParameterName = "$Now";
            p.Value = ToTimestamp();

            cmd.Parameters.Add(p);


            await cmd.ExecuteNonQueryAsync(ct);
            await trx.CommitAsync(ct);

            var dur = Stopwatch.GetTimestamp() - start;
            var ts = TimeSpan.FromTicks(dur);
            logger.DebugFormat("Cleanup Task completed in {0} ms", ts.TotalMilliseconds);


        }
        catch (Exception cause)
        {
            logger.Error(cause, "Failed to execute Cleanup Task");
        }
        finally
        {
            Lock.Release();
        }


        return true;

    }



    private static readonly string LogEventDdl =
        """
        CREATE TABLE if not exists LogEvents (
            Id            INTEGER PRIMARY KEY AUTOINCREMENT,
            Domain        TEXT    NOT NULL,
            CorrelationId TEXT    NOT NULL,
            Category      TEXT    NOT NULL,
            Tenant        TEXT    NOT NULL,
            Subject       TEXT    NOT NULL,
            Tag           TEXT    NOT NULL,
            Level         INTEGER NOT NULL,
            Color         INTEGER NOT NULL,
            Nesting       INTEGER NOT NULL,
            Title         TEXT    NOT NULL,
            Type          INTEGER NOT NULL,
            Payload       TEXT    DEFAULT ('') NOT NULL,
            Occurred      INTEGER NOT NULL,
            TimeToLive    INTEGER NOT NULL
        )
        STRICT;

        CREATE INDEX if not exists ByDomainOccurred ON LogEvents (
            Domain,
            Occurred
        );
        
        CREATE INDEX if not exists ByDomainCorrelation ON LogEvents (
            Domain,
            CorrelationId
        );

        CREATE INDEX if not exists ByTimeToLive ON LogEvents (
            TimeToLive
        );
       
        """;

    private static readonly string LogEventInsertDml =
        """
        INSERT INTO LogEvents (
            Domain,
            CorrelationId,
            Category,
            Tenant,
            Subject,
            Tag,
            Level,
            Color,
            Nesting,
            Title,
            Type,
            Payload,
            Occurred,
            TimeToLive
        )
        values
        (
            $Domain,
            $CorrelationId,
            $Category,
            $Tenant,
            $Subject,
            $Tag,
            $Level,
            $Color,
            $Nesting,
            $Title,
            $Type,
            $Payload,
            $Occurred,
            $TimeToLive
        )
        """;

    private static readonly string LogEventCleanupDml =
        """
        DELETE FROM LogEvents WHERE TimeToLive < $Now
        """;


    private static readonly string LogEventsByRangeDml =
        """
        SELECT
        
            CorrelationId, 
            max(Subject) as Subject, 
            max(Level) as Level, 
            min(Occurred) as Begin, 
            max(Occurred) as End 
        
        FROM LogEvents 
        
        WHERE 
        
            Domain = $Domain
            and CorrelationId in (SELECT distinct CorrelationId FROM LogEvents WHERE Occurred BETWEEN $Begin AND $End ) GROUP BY CorrelationId
        """;


    private static readonly string LogEventsByCorrelationDml =
        """
        SELECT
            CorrelationId,
            Category,
            Tenant,
            Subject,
            Tag,
            Level,
            Color,
            Nesting,
            Title,
            Type,
            Payload,
            Occurred
        FROM
            LogEvents
        WHERE
            CorrelationId = $CorrelationId
        ORDER BY
            Occurred
        """;

}


public class CorrelationGroup
{

    public string CorrelationId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int Level { get; set; }
    public DateTime Begin { get; set; }
    public DateTime End { get; set; }

}

public class Packet
{


    public string Category { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;


    public string Title { get; set; } = string.Empty;


    public string Tenant { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;


    public int Level { get; set; }
    public int Color { get; set; }
    public int Nesting { get; set; }


    public int Type { get; set; }
    public string Payload { get; set; } = string.Empty;

    public DateTime Occurred { get; set; }


}