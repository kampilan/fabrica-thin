using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using System.Drawing;
using NUnit.Framework;

namespace Fabrica.Tests;

public class WatchTests
{


    [Test]
    public void Test_0620_0100_Default_Watch_Factory_Should_Log()
    {

        var factory = WatchFactoryLocator.StartBootFactory();

        var loggerPre = factory.GetLogger("Test");

        loggerPre.Warning("Pre Logging. Should be in Console");

        factory.Stop();

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







}