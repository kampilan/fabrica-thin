using System.Data.Common;
using Autofac;
using CommunityToolkit.Diagnostics;
using Fabrica.Persistence.Connections;
using Fabrica.Persistence.Outbox;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;

namespace Fabrica.Persistence;

public static class AutofacExtensions
{

    /// <summary>
    /// Registers a connection resolver in the Autofac container. The resolver establishes
    /// connections to both the origin and replica databases using the provided database
    /// provider factory and connection strings.
    /// </summary>
    /// <param name="builder">The Autofac <see cref="ContainerBuilder"/> instance to register the connection resolver with.</param>
    /// <param name="factory">The <see cref="DbProviderFactory"/> instance used to create database connections.</param>
    /// <param name="replicaConnectionStr">The connection string for the replica database.</param>
    /// <param name="originConnectionStr">The connection string for the origin database.</param>
    /// <returns>The updated <see cref="ContainerBuilder"/> instance for further configuration.</returns>
    public static ContainerBuilder AddConnectionResolver(this ContainerBuilder builder, DbProviderFactory factory, string replicaConnectionStr, string originConnectionStr)
    {

        Guard.IsNotNull(builder, nameof(builder));
        Guard.IsNotNull(factory, nameof(factory));
        Guard.IsNotNullOrWhiteSpace(replicaConnectionStr, nameof(replicaConnectionStr));
        Guard.IsNotNullOrWhiteSpace(originConnectionStr, nameof(originConnectionStr));  
        
        builder.Register(_ =>
            {

                var comp = new ConnectionResolver( factory, replicaConnectionStr, originConnectionStr );

                return comp;
            })
            .As<IConnectionResolver>()
            .SingleInstance();


        return builder;
    }


    /// <summary>
    /// Configures and registers persistence-related services in the Autofac container. This includes
    /// unit of work components, commit signal management, and required dependencies for persistence
    /// operations.
    /// </summary>
    /// <param name="builder">The Autofac <see cref="ContainerBuilder"/> instance to configure persistence services.</param>
    /// <param name="commitSignalDuration">
    /// An optional <see cref="TimeSpan"/> specifying the interval for the commit signal.
    /// Defaults to 100 seconds if not provided.
    /// </param>
    /// <returns>The updated <see cref="ContainerBuilder"/> instance for further configuration.</returns>
    public static ContainerBuilder UsePersistence(this ContainerBuilder builder, TimeSpan commitSignalDuration = default )
    {

        if( commitSignalDuration == TimeSpan.Zero )
            commitSignalDuration = TimeSpan.FromMilliseconds(100);    

        
        builder.Register(c =>
            {

                var comp = new WaitEventUowCommitSignal
                {
                    SignalInterval = commitSignalDuration
                };
                
                return comp;
                
            })
            .As<IUnitOfWorkCommitSignal>()
            .SingleInstance();


        
        // ************************************************
        builder.Register(c =>
            {

                var correlation = c.Resolve<ICorrelation>();
                var resolver    = c.Resolve<IConnectionResolver>();
                var signal      = c.Resolve<IUnitOfWorkCommitSignal>();
                
                var comp = new UnitOfWork.UnitOfWork(correlation, resolver, signal );
                return comp;

            })
            .As<IUnitOfWork>()
            .AsSelf()
            .InstancePerLifetimeScope();



        // ************************************************
        return builder;

    }    
    
}