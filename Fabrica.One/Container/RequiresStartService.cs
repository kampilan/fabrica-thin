
// ReSharper disable UnusedMember.Global

using Autofac;
using Fabrica.Utilities.Container;
using Microsoft.Extensions.Hosting;

namespace Fabrica.One.Container;

public class RequiresStartService(ILifetimeScope root) : IHostedService
{

    protected ILifetimeScope RootScope { get; } = root;


    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {

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
            throw;
        }

    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {

        return Task.CompletedTask;

    }


}