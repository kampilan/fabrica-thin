using System.Diagnostics;
using System.Drawing;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Fabrica.Watch.Sink;

var config = DefaultConfig.Instance.WithArtifactsPath("e:/benchmarks");
var summary = BenchmarkRunner.Run<WatchBenchmark>(config);

void Delay(TimeSpan dur)
{

    var ticks = dur.Ticks;
    var start = Stopwatch.GetTimestamp();
    long now;
    do
    {
        now = Stopwatch.GetTimestamp();

    } while ( (now-start) < ticks );

}


[MemoryDiagnoser]
public class WatchBenchmark
{

    const string Category = "Fabrica.Logging.Test";

    private static object TheModel { get; } = new { Test = 1, Very = true };
    private static string MethodName { get; } = "Loop";
    private static string VarName { get; } = "X";
    private static string ModelName { get; } = "TheModel";

    private static ILogger QuietLogger { get; } = new QuietLogger();

    private static MonitorSink TheSink { get; } = new (){Accumulate = false, WorkDelay = TimeSpan.MinValue};

    private static Correlation Corr { get; set; } = new ();


    [GlobalSetup]
    public void Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.InitialPoolSize = 100;
        maker.MaxPoolSize = 1000;
        maker.UseLocalSwitchSource()
            .WhenMatched(Category, "", Level.Debug, Color.Bisque)
            .WhenMatched("WatchBenchmark", "", Level.Debug, Color.Bisque)
            .WhenNotMatched(Level.Quiet);

        maker.AddSink(new RealtimeSink());

        //maker.UseQuiet();

        maker.Build();

    }

    [GlobalCleanup]
    public async Task Cleanup()
    {


        WatchFactoryLocator.Factory.Stop();

        await Task.Delay(2000);

        await using var fs = new FileStream("e:/logs/output.txt", FileMode.Create, FileAccess.Write);
        await using var sw = new StreamWriter(fs);
        await sw.WriteLineAsync($"Sink Count: ({TheSink.Total})");


    }


    [Benchmark]
    public void LoggingBenchmark()
    {

        using var logger = WatchFactoryLocator.Factory.GetLogger<WatchBenchmark>();

        logger.Debug(VarName);

        logger.Inspect(VarName, 1);
   

    }

    [Benchmark]
    public void EnterMethodBenchmark()
    {

        using var logger = this.EnterMethod();

        logger.Debug(VarName);

        logger.Inspect(VarName, 1);


    }



}