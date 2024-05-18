using System.Drawing;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;

namespace Fabrica.Tests;

public class PatchTestModule : ServiceModule
{

    public PatchTestModule()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();

        maker.UseLocalSwitchSource()
            .WhenMatched("Microsoft", "", Level.Warning, Color.LightGreen)
            .WhenNotMatched(Level.Debug, Color.LightSalmon);

        maker.Build();

    }


    protected override void Load()
    {

    }

}