using System.Drawing;
using Autofac;
using Fabrica.Utilities.Pipeline;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using JetBrains.Annotations;
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
    public async Task Should_Resolve_Pipeline_And_Run_Pipeline()
    {

        using var logger = this.EnterMethod();
        
        await using var scope = TheRoot.BeginLifetimeScope();

        var builder = scope.Resolve<IPipelineBuilder<TestContext>>();
        builder.SetAction( async (_)=> await DoIt() );

        var pipeline = builder.Build();

        var context = new TestContext();
        
        await pipeline.ExecuteAsync(context);

        logger.LogObject(nameof(context), context);
        
        Assert.That(context.Success, Is.True);
        Assert.That(context.Phase, Is.EqualTo(PipelinePhase.After));
        Assert.That(context.FailedStep, Is.EqualTo(""));
        Assert.That(context.Cause, Is.Null);

        return;

        async Task DoIt()
        {
            using var innerlogger = this.GetLogger();
            innerlogger.Debug("Do It");
            await Task.Delay(1000);
            innerlogger.Debug("Done");
        }
        
    }    
    
    [Test]
    public async Task Should_Resolve_BadBefore_And_Run_Pipeline()
    {

        using var logger = this.EnterMethod();
        
        await using var scope = TheRoot.BeginLifetimeScope();

        var builder = scope.Resolve<IPipelineBuilder<BadBeforeContext>>();
        builder.SetAction( async (_)=> await DoIt());
        
        var pipeline = builder.Build();

        var context = new BadBeforeContext();
        
        await pipeline.ExecuteAsync(context);

        logger.LogObject(nameof(context), context);
        
        Assert.That(context.Success, Is.False);
        Assert.That(context.Phase, Is.EqualTo(PipelinePhase.Before));
        Assert.That(context.FailedStep, Is.EqualTo(typeof(StepBadBefore).GetConciseFullName()));
        Assert.That(context.Cause, Is.Not.Null);

        return;

        async Task DoIt()
        {
            using var innerlogger = this.GetLogger();
            innerlogger.Debug("Do It");
            await Task.Delay(1000);
            innerlogger.Debug("Done");
        }
        
    }    
    
    [Test]
    public async Task Should_Resolve_BadAfter_And_Run_Pipeline()
    {

        using var logger = this.EnterMethod();
        
        await using var scope = TheRoot.BeginLifetimeScope();

        var builder = scope.Resolve<IPipelineBuilder<BadAfterContext>>();
        builder.SetAction( async (_)=> await DoIt());        
        
        var pipeline = builder.Build();

        var context = new BadAfterContext();


        await pipeline.ExecuteAsync(context);
        
        logger.LogObject(nameof(context), context);        
        
        Assert.That(context.Success, Is.False);
        Assert.That(context.Phase, Is.EqualTo(PipelinePhase.After));
        Assert.That(context.FailedStep, Is.EqualTo(typeof(StepBadAfter).GetConciseFullName()));

        return;

        async Task DoIt()
        {
            using var innerlogger = this.GetLogger();
            innerlogger.Debug("Do It");
            await Task.Delay(1000);
            innerlogger.Debug("Done");
        }
        
    }    
    
    
    
    
    
    private class TheModule : Module
    {

        
        protected override void Load(ContainerBuilder builder)
        {


            builder.RegisterPipelineBuilder<TestContext>(steps =>
            {
                steps
                    .Add<StepOne<TestContext>>()
                    .Add<StepTwo<TestContext>>()
                    .Add<StepThree<TestContext>>();
            });

            builder.RegisterPipelineBuilder<BadBeforeContext>(steps =>
            {
                steps
                    .Add<StepOne<BadBeforeContext>>()
                    .Add<StepTwo<BadBeforeContext>>()
                    .Add<StepThree<BadBeforeContext>>()
                    .Add<StepBadBefore>();
            });

            builder.RegisterPipelineBuilder<BadAfterContext>(steps =>
            {
                steps
                    .Add<StepOne<BadAfterContext>>()
                    .Add<StepTwo<BadAfterContext>>()
                    .Add<StepThree<BadAfterContext>>()
                    .Add<StepBadAfter>();
            });

        }

        
    }
    
    
}

    



public class TestContext: BasePipelineContext, IPipelineContext
{
}

public class BadBeforeContext: BasePipelineContext, IPipelineContext
{
}

public class BadAfterContext: BasePipelineContext, IPipelineContext
{
}



[UsedImplicitly]
public class StepOne<TContext> : BasePipelineStep<TContext>, IPipelineStep<TContext> where TContext : class, IPipelineContext
{
    
    protected override Task Before(TContext context)
    {

        using var logger = this.EnterMethod();
        
        logger.Debug("Step 1: Before Next");
        
        return Task.CompletedTask;
        
    }

    
    protected override Task After(TContext context)
    {
        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step 1: After Next");
        
        return Task.CompletedTask;
        
    }
  
    
}

[UsedImplicitly]
public class StepTwo<TContext> : BasePipelineStep<TContext>, IPipelineStep<TContext> where TContext : class, IPipelineContext
{

    
    protected override Task Before(TContext context)
    {
        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step 2: Before Next");
        
        return Task.CompletedTask;
        
    }


    protected override Task After(TContext context)
    {
        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step 2: After Next");
        
        return Task.CompletedTask;
        
    }
    
}


[UsedImplicitly]
public class StepThree<TContext> : BasePipelineStep<TContext>, IPipelineStep<TContext> where TContext : class, IPipelineContext
{

    public StepThree()
    {
        ContinueAfterFailure = true;
    }
    
    protected override Task Before(TContext context)
    {
        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step 3: Before Next");
        
        return Task.CompletedTask;
        
    }

    protected override Task After(TContext context)
    {
        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step 3: After Next");
        
        return Task.CompletedTask;
        
    }
    
}


public class StepBadBefore : BasePipelineStep<BadBeforeContext>, IPipelineStep<BadBeforeContext>
{
    
    protected override Task Before(BadBeforeContext context)
    {

        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step Bad Before: Before Next");
        throw new Exception("I'm Bad Before");

        
    }

    
}


public class StepBadAfter : BasePipelineStep<BadAfterContext>, IPipelineStep<BadAfterContext>
{
    
    protected override Task Before(BadAfterContext context)
    {
        
        using var logger = this.EnterMethod();
        
        logger.Debug("Step Bad After: Before Next");

        return Task.CompletedTask;
        
    }

    protected override Task After(BadAfterContext context)
    {
        throw new Exception("I'm Bad After");
    }    
    
    
}

