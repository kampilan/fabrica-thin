using System.Diagnostics;
using System.Drawing;
using Autofac;
using Fabrica.Aws;
using Fabrica.Identity;
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
        
        Assert.That(dur.TotalMilliseconds, Is.LessThan(2100));
        
        await using var scope = TheRoot.BeginLifetimeScope();
        var service = scope.Resolve<IInstanceMetadata>();

        Assert.That( service.IsRunningOnEc2, Is.EqualTo(false));
        Assert.That( service.InstanceId, Is.EqualTo("1234567890"));

        await Task.Delay(35000);

    }


    private class TheModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {

            builder. UseAws("kampilan", imd =>
            {
                imd.Timeout = TimeSpan.FromSeconds(2);
                imd.InstanceId = "1234567890";
            });
            
        }
        
    }
    
    
    
}