using System.Drawing;
using System.Text.Json.Serialization;
using Autofac;
using Fabrica.Utilities.Pipeline;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using NUnit.Framework;

namespace Fabrica.Tests;

[TestFixture]
public class PipelineTests
{

    [OneTimeSetUp]
    public void Setup()
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
    public async Task Should_Build_And_Run_Pipeline()
    {

        var context = new TestContext(DoIt);

        async Task DoIt()
        {
            using var logger = this.GetLogger();
            logger.Debug("Do It");
            await Task.Delay(1000);
            logger.Debug("Done");
        }
        
        var builder = new CustomBuilder()
            .AddStep(new StepOne())
            .AddStep(new StepTwo())
            .AddStep(new StepThree())
            .AddStep(new StepEnd());    

        var pipeline = builder.Build();
        
        await pipeline.ExecuteAsync(context);
        
    }

    [Test]
    public async Task Should_Resolve_And_Run_Pipeline()
    {

        using var logger = this.EnterMethod();
        
        await using var scope = TheRoot.BeginLifetimeScope();

        var builder = scope.Resolve<CustomBuilder>();

        builder.AddStep(new StepEnd());
        var pipeline = builder.Build();

        var context = new TestContext(DoIt);

        async Task DoIt()
        {
            using var innerlogger = this.GetLogger();
            innerlogger.Debug("Do It");
            await Task.Delay(1000);
            innerlogger.Debug("Done");
        }
        
        
        await pipeline.ExecuteAsync(context);

        logger.LogObject(nameof(context), context);
        
        
        Assert.That(context.Success, Is.True);

        
    }
    
    [Test]
    public async Task Should_Resolve_Custom_And_Run_Pipeline()
    {

        using var logger = this.EnterMethod();
        
        await using var scope = TheRoot.BeginLifetimeScope();

        var builder = scope.Resolve<CustomBuilder>();
        var pipeline = builder.Build();

        var context = new TestContext(DoIt);
        
        async Task DoIt()
        {
            using var innerlogger = this.GetLogger();
            innerlogger.Debug("Do It");
            await Task.Delay(1000);
            innerlogger.Debug("Done");
        }
        
        await pipeline.ExecuteAsync(context);

        logger.LogObject(nameof(context), context);
        
        Assert.That(context.Success, Is.True);
        Assert.That(context.Phase, Is.EqualTo(PipelinePhase.After));
        Assert.That(context.FailedStep, Is.EqualTo(""));
        Assert.That(context.Cause, Is.Null);

    }    
    
    [Test]
    public async Task Should_Resolve_CustomBadBefore_And_Run_Pipeline()
    {

        using var logger = this.EnterMethod();
        
        await using var scope = TheRoot.BeginLifetimeScope();

        var builder = scope.Resolve<CustomBadBeforeBuilder>();
        var pipeline = builder.Build();

        var context = new TestContext(DoIt);

        async Task DoIt()
        {
            using var innerlogger = this.GetLogger();
            innerlogger.Debug("Do It");
            await Task.Delay(1000);
            innerlogger.Debug("Done");
        }
            
        
        await pipeline.ExecuteAsync(context);

        logger.LogObject(nameof(context), context);
        
        Assert.That(context.Success, Is.False);
        Assert.That(context.Phase, Is.EqualTo(PipelinePhase.Before));
        Assert.That(context.FailedStep, Is.EqualTo(typeof(StepBadBefore).GetConciseFullName()));
        Assert.That(context.Cause, Is.Not.Null);
        
    }    
    
    [Test]
    public async Task Should_Resolve_CustomBadAfter_And_Run_Pipeline()
    {

        using var logger = this.EnterMethod();
        
        await using var scope = TheRoot.BeginLifetimeScope();

        var builder = scope.Resolve<IPipelineBuilder<TestContext>>();
        var pipeline = builder.Build();

        var context = new TestContext(DoIt);

        async Task DoIt()
        {
            using var innerlogger = this.GetLogger();
            innerlogger.Debug("Do It");
            await Task.Delay(1000);
            innerlogger.Debug("Done");
        }
            
        
        await pipeline.ExecuteAsync(context);
        
        logger.LogObject(nameof(context), context);        
        
        Assert.That(context.Success, Is.False);
        Assert.That(context.Phase, Is.EqualTo(PipelinePhase.After));
        Assert.That(context.FailedStep, Is.EqualTo(typeof(StepBadAfter).GetConciseFullName()));

        
    }    
    
    
    
    
    
    private class TheModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {


            builder.AddPipelineBuilder<CustomBuilder,TestContext>(plb =>
            {

                plb.AddStep(new StepOne())
                   .AddStep(new StepTwo())
                   .AddStep(new StepThree());
                
            });

            builder.AddPipelineBuilder<CustomBadBeforeBuilder,TestContext>(plb =>
            {

                plb.AddStep(new StepOne())
                    .AddStep(new StepTwo())
                    .AddStep(new StepThree());
                
            });
            
            builder.AddPipelineBuilder<CustomBadAfterBuilder,TestContext>(plb =>
            {

                plb.AddStep(new StepOne())
                    .AddStep(new StepTwo())
                    .AddStep(new StepThree());
                
            });
            
            
            
        }
        
    }
    
    
}

public class CustomBuilder : BasePipelineBuilder<TestContext>
{

    public override Pipeline<TestContext> Build()
    {
        AddStep(new StepEnd());
        return base.Build();
    }
    
}    

public class CustomBadBeforeBuilder : BasePipelineBuilder<TestContext>
{

    public override Pipeline<TestContext> Build()
    {
        
        AddStep(new StepBadBefore());
        AddStep(new StepEnd());
        return base.Build();
    }
    
}

public class CustomBadAfterBuilder : BasePipelineBuilder<TestContext>
{

    public override Pipeline<TestContext> Build()
    {
        
        AddStep(new StepBadAfter());
        AddStep(new StepEnd());
        return base.Build();
    }
    
}





public class TestContext( Func<Task> dotIt ): BasePipelineContext, IPipelineContext
{
    
    [JsonIgnore]
    public Func<Task> DoIt { get; } = dotIt;


}

public class StepOne : BasePipelineStep<TestContext>, IPipelineStep<TestContext>
{
    
    protected override Task Before(TestContext context)
    {

        using var logger = this.EnterMethod();
        
        logger.Debug("Step 1: Before Next");
        
        return Task.CompletedTask;
        
    }

/*    
    protected override Task After(TestContext context)
    {
        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step 1: After Next");
        
        return Task.CompletedTask;
        
    }
*/  
    
}

public class StepTwo : BasePipelineStep<TestContext>, IPipelineStep<TestContext>
{

/*    
    protected override Task Before(TestContext context)
    {
        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step 2: Before Next");
        
        return Task.CompletedTask;
        
    }
*/

    protected override Task After(TestContext context)
    {
        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step 2: After Next");
        
        return Task.CompletedTask;
        
    }
    
}


public class StepThree : BasePipelineStep<TestContext>, IPipelineStep<TestContext>
{

    public StepThree()
    {
        ContinueOnFailure = true;
    }
    
    protected override Task Before(TestContext context)
    {
        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step 3: Before Next");
        
        return Task.CompletedTask;
        
    }

    protected override Task After(TestContext context)
    {
        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step 3: After Next");
        
        return Task.CompletedTask;
        
    }
    
}


public class StepBadBefore : BasePipelineStep<TestContext>, IPipelineStep<TestContext>
{
    
    protected override Task Before(TestContext context)
    {

        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step Bad Before: Before Next");
        throw new Exception("I'm Bad Before");

        
    }

    
}


public class StepBadAfter : BasePipelineStep<TestContext>, IPipelineStep<TestContext>
{
    
    protected override Task Before(TestContext context)
    {
        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step Bad After: Before Next");

        return Task.CompletedTask;
        
    }

    protected override Task After(TestContext context)
    {
        throw new Exception("I'm Bad After");
    }    
    
    
}


public class StepEnd : IPipelineStep<TestContext>
{

    public async Task InvokeAsync( TestContext context, Func<TestContext,Task> next )
    {
        
        await context.DoIt();
        context.Phase = PipelinePhase.After;
        
    }

    
}
