using System.Data.Common;
using Autofac;
using Fabrica.App.Persistence.Repository;
using Fabrica.Persistence.Connections;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using Microsoft.EntityFrameworkCore;
using SmartFormat;

namespace Fabrica.App.Persistence;

public static class AutofacExtensions
{

    
    
    
    
    
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