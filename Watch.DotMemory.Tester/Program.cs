using System.Drawing;
using Fabrica.Watch;
using Fabrica.Watch.Sink;

const string Category = "Fabrica.Logging.Test";
const string VarName = "X";

var TheSink  = new MonitorSink { Accumulate = false, WorkDelay = TimeSpan.MinValue };

var maker = new WatchFactoryBuilder();
maker.InitialPoolSize = 100;
maker.MaxPoolSize = 1000;
maker.UseLocalSwitchSource()
    .WhenMatched(Category, "", Level.Debug, Color.Bisque)
    .WhenMatched("Program", "", Level.Debug, Color.Bisque)
    .WhenNotMatched(Level.Quiet);

maker.Sink = TheSink;

//maker.UseQuiet();

maker.BuildAsync();


Console.WriteLine("Press ESC to stop");
do
{

    using var logger = WatchFactoryLocator.Factory.GetLogger<Program>();

    logger.Debug(VarName);

    logger.Inspect(VarName, 1);

    Console.WriteLine($"Count: {TheSink.Total}");

} while (Console.ReadKey(true).Key != ConsoleKey.Escape);
