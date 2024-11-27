using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Sink;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Drawing;

namespace Fabrica.Tests;

public class LogEventBatchSerializationTests
{

    const string Category = "Fabrica.Logging.Test";
    private static string VarName { get; } = "X";


    private static MonitorSink TheSink { get; } = new() { Accumulate = true, WorkDelay = TimeSpan.MinValue };

    private static Correlation Corr { get; set; } = new();

    [OneTimeSetUp]
    public void Setup()
    {


        var maker = new WatchFactoryBuilder();
        maker.InitialPoolSize = 100;
        maker.MaxPoolSize = 1000;
        maker.UseLocalSwitchSource()
            .WhenMatched(Category, "", Level.Debug, Color.Bisque)
            .WhenMatched("Fabrica.Tests.LogEventBatchSerializationTests", "", Level.Debug, Color.Bisque)
            .WhenNotMatched(Level.Quiet);

        maker.AddSink(TheSink);

        //maker.UseQuiet();

        maker.Build();

    }




    [Test]
    public async Task Test_0610_0100_Should_Roundtrip_With_MemoryPack()
    {


        var logger = Corr.EnterMethod<LogEventBatchSerializationTests>();

        // *****************************************************************
        logger.Debug("Attempting to try something");

        logger.Inspect(nameof(VarName), VarName);

        logger.Dispose();



        var batch = new LogEventBatch();
        batch.Events.AddRange(TheSink.GetEvents());

        using var ms = new MemoryStream();

        await LogEventBatchSerializer.ToStream(batch, ms);

        ms.Seek(0, SeekOrigin.Begin);

        var batch2 = await LogEventBatchSerializer.FromStream(ms);


    }


    [Test]
    public void Test_0610_0100_Should_Roundtrip_With_Json()
    {

        var logger = Corr.EnterMethod<LogEventBatchSerializationTests>();

        // *****************************************************************
        logger.Debug("Attempting to try something");

        logger.Inspect(nameof(VarName), VarName);

        logger.Dispose();


        var batch = new LogEventBatch();
        batch.Events.AddRange(TheSink.GetEvents());

        var json = LogEventBatchSerializer.ToJson(batch);

        var batch2 = LogEventBatchSerializer.FromJson(json);


    }




}