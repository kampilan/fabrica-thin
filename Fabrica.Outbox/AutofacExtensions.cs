using Autofac;
using Fabrica.Persistence.Outbox;

namespace Fabrica.Outbox;

public static class AutofacExtensions
{

    public static ContainerBuilder RegisterOutboxSignal(this ContainerBuilder builder)
    {

        builder.RegisterType<OutboxSignal>()
            .As<IOutboxSignal>()
            .SingleInstance();
        
        return builder;
        
    }
    
    
}