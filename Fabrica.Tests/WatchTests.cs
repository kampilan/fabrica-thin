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
    public void Test_0620_0200_Watch_Factory_Should_Start_and_Stopped()
    {


        var maker = WatchFactoryBuilder.Create();

        maker.UseLocalSwitchSource()
            .WhenMatched("Microsoft", "", Level.Warning, Color.LightGreen)
            .WhenNotMatched(Level.Debug, Color.LightSalmon);

        maker.UseRealtime();

        maker.Build();

        var logger = WatchFactoryLocator.Factory.GetLogger("Test");
        logger.Debug("Testing");

        logger.Dispose();

        WatchFactoryLocator.Factory.Stop();



        maker.Build();

        var logger2 = WatchFactoryLocator.Factory.GetLogger("Test");
        logger2.Debug("Testing");

        logger2.Dispose();

        WatchFactoryLocator.Factory.Stop();


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
    public async Task Test_0620_0500_Should_Send_To_Relay()
    {

    
        var maker = WatchFactoryBuilder.Create();
        maker.UseForegroundFactory();
        maker.UseRelaySink();
        
        maker.UseLocalSwitchSource()
            .WhenMatched("Fabrica.Tests", "", Level.Debug, Color.LightGreen)
            .WhenNotMatched(Level.Warning, Color.LightSalmon);

        maker.Build();

        using var logger = this.EnterMethod();

        logger.Debug("Attempting to log just a test event]");

        await Task.Delay(TimeSpan.FromSeconds(3));

        
    }
    
    
    
    
    


}