using System.Data.Common;
using Autofac;
using Fabrica.App.Persistence.Connections;
using Fabrica.App.Persistence.Repository;
using Fabrica.App.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using Microsoft.EntityFrameworkCore;
using SmartFormat;

namespace Fabrica.App.Persistence;

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
        builder.Register(c =>
            {
                var correlation = c.Resolve<ICorrelation>();

                var comp = new ConnectionResolver(correlation, factory, replicaConnectionStr, originConnectionStr);

                return comp;
            })
            .As<IConnectionResolver>()
            .InstancePerLifetimeScope();


        return builder;
    }

    
    
    /// <summary>
    /// Registers a connection resolver in the Autofac container. The resolver dynamically resolves
    /// connections to both the origin and replica databases by formatting the provided connection string templates
    /// with the specified model using SmartFormat.
    /// </summary>
    /// <param name="builder">The Autofac <see cref="ContainerBuilder"/> instance to register the connection resolver with.</param>
    /// <param name="factory">The <see cref="DbProviderFactory"/> instance used to create database connections.</param>
    /// <param name="replicaConnectionTemplate">The template for the replica database connection string.</param>
    /// <param name="originConnectionTemplate">The template for the origin database connection string.</param>
    /// <param name="model">The object used for formatting the connection string templates.</param>
    /// <returns>The updated <see cref="ContainerBuilder"/> instance for further configuration.</returns>
    public static ContainerBuilder AddConnectionResolver(this ContainerBuilder builder, DbProviderFactory factory, string replicaConnectionTemplate, string originConnectionTemplate, object model)
    {

        builder.Register(c =>
            {
                var correlation = c.Resolve<ICorrelation>();

                var replica = Smart.Format(replicaConnectionTemplate, model);
                var origin = Smart.Format(originConnectionTemplate, model);

                var comp = new ConnectionResolver(correlation, factory, replica, origin);

                return comp;
            })
            .As<IConnectionResolver>()
            .InstancePerLifetimeScope();


        return builder;
        
    }
    
    
    public static ContainerBuilder UsePersistence(this ContainerBuilder builder )
    {


        // ************************************************
        builder.Register(c =>
            {

                var correlation = c.Resolve<ICorrelation>();
                var resolver    = c.Resolve<IConnectionResolver>();

                var comp = new UnitOfWork.UnitOfWork(correlation, resolver);
                return comp;

            })
            .As<IUnitOfWork>()
            .AsSelf()
            .InstancePerLifetimeScope();



        // ************************************************
        return builder;

    }    
    
    
    
    public static ContainerBuilder RegisterDbRepositories<TOrigin,TReplica>(this ContainerBuilder builder) where TOrigin: DbContext where TReplica: DbContext
    {
        
        builder.Register(c =>
            {
                var corr = c.Resolve<ICorrelation>();
                var dbCtx = c.Resolve<TOrigin>();

                var comp = new OriginRepository( corr, dbCtx );

                return comp;

            })
            .As<IOriginRepository>()
            .InstancePerLifetimeScope();        

        
        builder.Register(c =>
            {
                var corr = c.Resolve<ICorrelation>();
                var dbCtx = c.Resolve<TReplica>();

                var comp = new ReplicaRepository( corr, dbCtx );

                return comp;

            })
            .As<IReplicaRepository>()
            .InstancePerLifetimeScope();

        return builder;

    }    
    
    
}