using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Utilities.Container;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Identity.Client;

public static class AutofacExtensions
{
    /// <summary>
    /// Adds an in-memory implementation of <see cref="ICredentialGrantRepository"/> to the <see cref="ContainerBuilder"/> and registers the provided credential grants.
    /// </summary>
    /// <param name="builder">The <see cref="ContainerBuilder"/> to which the in-memory credential grant repository will be added.</param>
    /// <param name="grants">A collection of <see cref="ICredentialGrant"/> instances to be registered in the in-memory repository.</param>
    /// <returns>The updated <see cref="ContainerBuilder"/> instance.</returns>
    public static ContainerBuilder AddMemoryCredentialGrantRepository(this ContainerBuilder builder, IEnumerable<ICredentialGrant> grants)
    {

        builder.Register(c =>
            {

                var comp = new MemoryCredentialGrantRepository();

                foreach (var grant in grants)
                    comp.AddGrant(grant);
                
                return comp;
                
            })
            .AsSelf()
            .As<ICredentialGrantRepository>()
            .SingleInstance();

        return builder;   
        
    }

    
    public static ContainerBuilder AddOidcTokenProducer(this ContainerBuilder builder)
    {

        // *************************************************************************        
        builder.Register(c =>
            {

                var http = c.Resolve<IHttpClientFactory>();
                var comp = new OidcTokenProducer(http);

                return comp;
                
            })
            .AsSelf()
            .As<ITokenProducer>()
            .SingleInstance();

        return builder;

    }    
    

    public static ContainerBuilder AddTokenSource(this ContainerBuilder builder, string grantName, TimeSpan renewalWindow=default)
    {

        if (renewalWindow == TimeSpan.Zero)
            renewalWindow = TimeSpan.FromMinutes(1);
        
        
        // *************************************************************************
        builder.Register(c =>
            {

                var corr = c.Resolve<ICorrelation>();
                var fact = c.Resolve<ITokenProducer>();
                var repo = c.Resolve<ICredentialGrantRepository>();
                
                var comp = new TokenSource( corr, grantName, repo, fact )
                {
                    RenewalWindow = renewalWindow
                };

                return comp;

            })
            .AsSelf()
            .Named<ITokenSource>( grantName )
            .As<IRequiresStart>()
            .SingleInstance()
            .AutoActivate();

        return builder;   
        
    }


    public static ContainerBuilder AddHttpClientWithGrant( this ContainerBuilder builder, string name, string grant, Action<HttpClient>? configure=null )
    {


        
        // *************************************************************************        
        builder.Register(c =>
            {
                var source = c.ResolveNamed<ITokenSource>(grant);
                var comp = new TokenSourceHttpHandler(source);
                return comp;
            })
            .AsSelf()
            .InstancePerDependency();


        
        // *************************************************************************        
        var services = new ServiceCollection();

        if (configure is not null)
        {
            services.AddHttpClient(name, configure)
                .AddHttpMessageHandler<TokenSourceHttpHandler>();
        }
        else
        {
            services.AddHttpClient(name)
                .AddHttpMessageHandler<TokenSourceHttpHandler>();
        }
        
        
        builder.Populate(services);


        
        // *************************************************************************        
        return builder;

    }    
    



}
