using System.Diagnostics;
using System.Drawing;
using Autofac;
using Fabrica.Aws;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using NUnit.Framework;

namespace Fabrica.Tests;

[TestFixture]
public class AwsTests
{

    [OneTimeSetUp]
    public async Task Setup()
    {

        var maker = WatchFactoryBuilder.Create();

        maker.UseLocalSwitchSource()
            .WhenNotMatched(Level.Trace, Color.LightSalmon);

        maker.UseRealtime();

        maker.Build();        

        var builder = new ContainerBuilder();
        builder.RegisterModule<TheModule>();

        TheRoot = builder.Build();
        
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        TheRoot.Dispose();
        
        await Task.Delay(500);
        await WatchFactoryLocator.Factory.Stop();
        
    }

    private IContainer TheRoot { get; set; } = null!;
    
    
    
    [Test]
    public async Task Should_Fail_Quickly()
    {
        
        var sw = Stopwatch.GetTimestamp();

        await TheRoot.StartComponents();
        
        var dur = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - sw);
        
        Assert.That(dur.TotalMilliseconds, Is.LessThan(31000));

    }


    private class TheModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {

            builder. UseAws("kampilan", TimeSpan.FromSeconds(45));
            
        }
        
    }
    
    
    
}