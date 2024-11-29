using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Watch.Sink;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Polly.Retry;


// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Fabrica.Watch.Http.Sink;

public abstract class AbstractHttpEventSinkProvider: IEventSinkProvider
{

    private static readonly string Key = "Fabrica.Watch.Http.Sink";

    public string SinkEndpoint { get; set; } = "";
    public string Domain { get; set; } = "";

    private IContainer Container { get; set; } = null!;
    private IHttpClientFactory Factory { get; set; } = null!;

    protected ConsoleEventSink DebugSink { get; } = new();

    protected abstract Task<HttpContent> BuildContentAsync(LogEventBatch batch);


    public Task Start()
    {

        var builder = new ContainerBuilder();

        var services = new ServiceCollection();

        services.AddResiliencePipeline( Key, pb =>
        {

            pb.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2, 
                BackoffType = DelayBackoffType.Constant, 
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(50)
            });

        });

        services.AddHttpClient( Key, c =>
        {

            if( !string.IsNullOrWhiteSpace(SinkEndpoint) )
                c.BaseAddress = new Uri(SinkEndpoint);

        });


        builder.Populate(services);

        Container = builder.Build();

        Factory = Container.Resolve<IHttpClientFactory>();


        return Task.CompletedTask;

    }

    public Task Stop()
    {

        Container.Dispose();

        return Task.CompletedTask;

    }


    public async Task Accept( LogEventBatch batch, CancellationToken ct=default )
    {

        using var logger = DebugSink.GetLogger<AbstractHttpEventSinkProvider>();

        try
        {


            if( string.IsNullOrWhiteSpace(batch.Domain) )
                batch.Domain = Domain;

            logger.Inspect(nameof(batch.Uid), batch.Uid);
            logger.Inspect(nameof(batch.Domain), batch.Domain);


            await using var scope = Container.BeginLifetimeScope();



            // *****************************************************************
            logger.Debug("Attempting to resolve Resilience Pipeline");
            var pp = scope.Resolve<ResiliencePipelineProvider<string>>();
            var pipeline = pp.GetPipeline(Key);



            // *****************************************************************
            logger.Debug("Attempting to resolve HTTP Factory");
            var factory = scope.Resolve<IHttpClientFactory>();
            using var client = factory.CreateClient();



            // *****************************************************************
            logger.Debug("Attempting to serialize batch into HttpContent");
            var content = await BuildContentAsync(batch);



            // *****************************************************************
            logger.Debug("Attempting to post batch");
            await pipeline.ExecuteAsync(async t =>
            {
                await client.PostAsync("", content, t);

            }, ct);


        }
        catch (Exception cause )
        {
            logger.Error(cause, "Failed to accept batch");
        }


    }


}