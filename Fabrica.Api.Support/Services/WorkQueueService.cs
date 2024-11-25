using Autofac;
using Fabrica.Utilities.Queue;
using Fabrica.Utilities.Types;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Text.Json;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;
using Fabrica.Identity;

namespace Fabrica.Api.Services;


[AttributeUsage(AttributeTargets.Class)]
public class WorkAttribute(string topic) : Attribute
{
    public string Topic { get; init; } = topic;
}


public class WorkRequestTypeSource : TypeSource
{

    protected override Func<Type, bool> GetPredicate()
    {
        return t => t is { IsClass: true, IsAbstract: false } && t.GetCustomAttribute(typeof(WorkAttribute)) is not null;
    }

}

public class WorkerEntry
{
    public string Topic { get; set; } = string.Empty;
    public Type Request { get; set; } = null!;

}


public class WorkQueueService( ILifetimeScope rootScope, IEnumerable<WorkRequestTypeSource> typeSources, IWorkMessageSource source ) : BackgroundService
{


    protected ImmutableDictionary<string, WorkerEntry> Workers { get; private set; } = null!;


    public override async Task StartAsync(CancellationToken cancellationToken)
    {

        using var logger = this.EnterMethod();

        var entries = new Dictionary<string, WorkerEntry>();

        foreach( var type in typeSources.SelectMany(s=>s.GetTypes()) )
        {

            var attr = type.GetCustomAttribute<WorkAttribute>();
            if (attr is null)
                continue;


            var entry = new WorkerEntry
            {
                Topic   = attr.Topic,
                Request = type
            };

            entries.Add(attr.Topic, entry );

        }


        logger.LogObject(nameof(entries), entries);


        Workers = ImmutableDictionary.CreateRange(entries);

        await base.StartAsync(cancellationToken);


    }


    protected virtual CompositeRequest MapMessageToRequest(WorkQueueMessage message)
    {

        using var logger = this.EnterMethod();


        // *****************************************************************
        logger.DebugFormat("Attempting to lookup worker request for topic: ({0)", message.Topic);
        if (!Workers.TryGetValue(message.Topic, out var entry))
        {
            logger.WarningFormat("Could not find a worker request for topic: ({0)", message.Topic);
            return new CompositeRequest();
        }



        // *****************************************************************
        logger.Debug("Attempting to deserialize worker request");
        var obj = JsonSerializer.Deserialize(message.Body, entry.Request);
        if (obj is null || obj is not IRequest<Response> request)
        {
            logger.WarningFormat("Could not deserialize worker request for topic: ({0)", message.Topic);
            return new CompositeRequest();
        }



        // *****************************************************************
        logger.Debug("Attempting to build CompositeRequest");
        var composite = new CompositeRequest();
        composite.Components.Add(request);



        // *****************************************************************
        return composite;


    }


    protected override async Task ExecuteAsync( CancellationToken mustStop )
    {

        while( !mustStop.IsCancellationRequested )
        {

            var (has, message, handle) = await source.Get(mustStop);

            if( !has || message is null || handle is null )
            {
                await Task.Delay(100, mustStop);
                continue;
            }


            using var logger = this.EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to map message to Composite request");
            var composite = MapMessageToRequest(message);



            // *****************************************************************
            logger.Debug("Attempting to begin lifetime scope");
            await using var scope = rootScope.BeginLifetimeScope();



            // *****************************************************************
            logger.Debug("Attempting to prepare correlation");
            var correlation = scope.Resolve<ICorrelation>();
            if( correlation is Correlation impl )
            {
                var ci = new FabricaIdentity(message.Claims);
                var cp = new ClaimsPrincipal(ci);

                impl.Caller = cp;

            }



            // *****************************************************************
            logger.Debug("Attempting to resolver mediator");
            var mediator = scope.Resolve<IRequestMediator>();



            // *****************************************************************
            logger.Debug("Attempting to send request vis mediator");
            var response = await mediator.Send( composite, mustStop );

            logger.LogObject(nameof(response), response);



            // *****************************************************************
            logger.Debug("Attempting to complete the message");
            await source.Complete( handle, mustStop );


        }

    }

}