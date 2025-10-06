using System.Diagnostics;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using System.Drawing;
using Fabrica.Utilities.Types;
using Fabrica.Watch.Http;
using Fabrica.Watch.Sink;
using Fabrica.Watch.SqlIte;
using Fabrica.Watch.Utilities;
using NUnit.Framework;

namespace Fabrica.Tests;

public class WatchTests
{


    [Test]
    public void Test_0620_0100_Default_Watch_Factory_Should_Log()
    {

        using var logger = WatchFactoryLocator.Factory.GetLogger<WatchTests>();

        logger.Debug("Pre Logging. Should be in Console");


    }


    [Test]
    public async Task Test_0620_0200_Watch_Factory_Should_Start_and_Stopped()
    {

        var maker = WatchFactoryBuilder.Create();

        maker.UseBinaryHttpSink("http://localhost:8181", "Fabrica.Watch.Development" );

        maker.Build();
        
        await Task.Delay(2000);
        
        
        var logger = WatchFactoryLocator.Factory.GetLogger("Fabrica.Pump");
        logger.Trace("Testing");

        logger.Dispose();

        await Task.Delay(2000);
        await WatchFactoryLocator.Factory.Stop();

/*
        maker.Build();

        var logger2 = WatchFactoryLocator.Factory.GetLogger("Fabrica.Watch.Features");
        logger2.Debug("Testing");

        logger2.Dispose();

        WatchFactoryLocator.Factory.Stop();
*/

    }

    [Test]
    public async Task Test_0620_0300_Sqlite_Sink_Should_Open_And_Create_Table()
    {

        var sink = new SqliteSink();
        sink.ConnectionString = "Data Source=e:/watch/test123.db";
        sink.Domain = "Test123";

        await sink.Start();
        await sink.Stop();

    }


    [Test]
    public async Task Test_0620_0400_Sqlite_Sink_Should_Save_Events()
    {

        var sink = new SqliteSink();
        sink.ConnectionString = "Data Source=e:/watch/test123.db";
        sink.Domain = "Test123";

        sink.QuietLogging = false;

        sink.UseWriteAheadLogJournaling = true;

        //sink.CleanupCron = "0,10,20,30,40,50 * * * *";
        sink.DebugTimeToLive = TimeSpan.FromMinutes(1);

        await sink.Start();

        var batch = new LogEventBatch {DomainUid = "Test123"};

        for( var i = 0; i < 26667; i++ )
        {

            var le = new LogEvent
            {
                Category = "Fabrica.Diagnostics.Http",
                CorrelationId = Ulid.NewUlid(),
                Title = "Begin Request",
                Tenant = "",
                Subject = "me@jamesmoring.com",
                Tag = "",
                Level = (int)Level.Debug,
                Color = Color.Bisque.ToArgb(),
                Nesting = 0,
                Type = 0,
                Payload = "",
                Occurred = DateTime.UtcNow
            };

            batch.Events.Add(le);

        }

        using var ms = new MemoryStream();
        await LogEventBatchSerializer.ToStream(batch, ms);

        ms.Seek(0, SeekOrigin.Begin);

        await sink.Accept(ms);

        //await Task.Delay(TimeSpan.FromMinutes(4));

        await sink.Stop();

    }


    [Test]
    public async Task Test_0620_0500_Sqlite_Sink_Should_Return_Correlation_Groups()
    {

        var sink = new SqliteSink();
        sink.ConnectionString = "Data Source=e:/watch/test123.db";
        sink.Domain = "Test123";

        sink.QuietLogging = false;

        sink.UseWriteAheadLogJournaling = true;

        //sink.CleanupCron = "0,10,20,30,40,50 * * * *";
        sink.DebugTimeToLive = TimeSpan.FromMinutes(1);

        await sink.Start();

        var groups = await sink.GetCorrelationGroups("Test123", DateTime.Now.AddHours(-3), DateTime.Now);

        await sink.Stop();

    }


    [Test]
    public async Task Test_0620_0600_Should_Not_Log_Sensitive_Property()
    {

        var maker = WatchFactoryBuilder.Create();

        maker.UseLocalSwitchSource()
            .WhenNotMatched(Level.Trace, Color.LightSalmon);

        maker.UseRealtime();

        maker.Build();
        await Task.Delay(3000);

        
        var logger = WatchFactoryLocator.Factory.GetLogger("Test");
        logger.Debug("Testing");

        var secret = new Secret {Name = "Test", Value = "1234567890098765432"};
        logger.LogObject(nameof(secret), secret);
        
        logger.Dispose();

        await Task.Delay(1000);
        await WatchFactoryLocator.Factory.Stop();

    }
    


}

public class Secret
{

    public string Name { get; set; } = string.Empty;

    [Sensitive]
    public string Value { get; set; } = string.Empty;
    
    
}