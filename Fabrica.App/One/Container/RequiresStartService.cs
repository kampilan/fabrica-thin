
// ReSharper disable UnusedMember.Global

using Autofac;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.Extensions.Hosting;

namespace Fabrica.App.One.Container;

public class RequiresStartService(ILifetimeScope root) : IHostedService
{

    protected ILifetimeScope RootScope { get; } = root;


    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {

        using var logger = this.EnterMethod();


        var currentStartable = "";
        try
        {
            // *****************************************************************
            var startables = RootScope.Resolve<IEnumerable<IRequiresStart>>();
            foreach (var c in startables)
            {
                currentStartable = c.GetType().FullName;
                await c.Start();
            }

        }
        catch (Exception cause)
        {
            var ctx = new { FailedStartable = currentStartable };
            logger.ErrorWithContext(cause, ctx, "RequiresStart failed");
            throw;
        }

    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {

        return Task.CompletedTask;

    }


}