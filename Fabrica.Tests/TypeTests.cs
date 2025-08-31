using System.Drawing;
using System.Text;
using Fabrica.Utilities.Cache;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using NUnit.Framework;

namespace Fabrica.Tests;

[TestFixture]
public class TypeTests
{

    [Test]
    public async Task Test_0600_0100_Should_Create_Concise_Name()
    {

        
        var maker = WatchFactoryBuilder.Create();

        maker.UseLocalSwitchSource()
            .WhenNotMatched(Level.Trace, Color.LightSalmon);

        maker.UseRealtime();

        maker.Build();        
        
        
        var x = new ConcurrentResource<string>(() =>
            {
                var rr = new RenewedResource<string>()
                {
                    Value = "Hello",
                    TimeToLive = TimeSpan.FromSeconds(60),
                    TimeToRenew = TimeSpan.FromSeconds(55)
                    
                };
                return Task.FromResult<IRenewedResource<string>>(rr);
            }
        );


        var name = x.GetType().GetConciseFullName();

        var logger =x.GetLogger();

        logger.EnterMethod("Testing");        
        logger.Debug("Testing");
        
        logger.Dispose();

        await x.Initialize();
       
        
        await Task.Delay(1000);
        await WatchFactoryLocator.Factory.Stop();
        


    }
    
    
}