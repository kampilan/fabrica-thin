using System.Drawing;
using Autofac;
using Fabrica.Aws;
using Fabrica.Identity.Client;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Fabrica.Tests;

public class TokenSourceTests
{

    [OneTimeSetUp]
    public async Task Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource()
            .WhenNotMatched(Level.Trace, Color.Aqua);

        maker.Build();

        var builder = new ContainerBuilder();
        builder.RegisterModule<TheModule>();


        TheContainer = builder.Build();

        await TheContainer.StartComponents();


    }

    [OneTimeTearDown]
    public async Task Teardown()
    {

        TheContainer.Dispose();
        await Task.Delay(1000);
        await WatchFactoryLocator.Factory.Stop();
        
    }


    private IContainer TheContainer { get; set; } = null!;

    
    [Test]
    public async Task CredentialGrant_Should_Be_Returned_From_Repository()
    {
        
        await using var scope = TheContainer.BeginLifetimeScope();
        var repo = scope.Resolve<ICredentialGrantRepository>();
        
        var grant = await repo.GetGrant("qbo-fortium-us-sandbox");

        Assert.That(grant,Is.Not.Null);

        var factory = scope.Resolve<ITokenProducer>();
        var token = await factory.FetchNew(grant);

        Assert.That(token,Is.Not.Null);
        
        await repo.UpdateGrant(grant);

    }
    
    
    [Test]
    public async Task TokenFactory_Should_Fetch_New_Token_From_Grant()
    {
        
        await using var scope = TheContainer.BeginLifetimeScope();

        var grant = new CredentialGrant
        {
            Kind = CredentialGrantKind.ResourceOwner,
            MetaEndpoint = "https://identity.ilumisolutions.net/realms/meshtek/.well-known/openid-configuration",
            ClientId = "account",
            ClientSecret = "huorMoowXCdZKZbn8t27M6H8OVrY30tu",
            UserName = "vaibhav.home@ilumi.co",
            Password = "vbmistry"
        };
        
        var factory = scope.Resolve<ITokenProducer>();
        var token = await factory.FetchNew(grant);

        Assert.That(token,Is.Not.Null);

        Assert.That(grant.AccessTokenUid,Is.Not.Null);
        Assert.That(grant.AccessTokenUid,Is.Not.Empty);
        Assert.That(grant.AccessTokenUid,Is.Not.WhiteSpace);        

        Assert.That(grant.AccessToken,Is.Not.Null);
        Assert.That(grant.AccessToken,Is.Not.Empty);
        Assert.That(grant.AccessToken,Is.Not.WhiteSpace);        
        
        Assert.That(grant.RefreshToken,Is.Not.Null);
        Assert.That(grant.RefreshToken,Is.Not.Empty);
        Assert.That(grant.RefreshToken,Is.Not.WhiteSpace);        
    
    }

    [Test]
    public async Task AccessTokenSource_Should_Return_Token()
    {

        await using var scope = TheContainer.BeginLifetimeScope();
        var source1 = scope.ResolveNamed<ITokenSource>( "qbo-fortium-us-sandbox" );
        
        var token1 = await source1.GetToken();

        Assert.That(token1,Is.Not.WhiteSpace);

        var source2 = scope.ResolveNamed<ITokenSource>( "meshtek-prod-ro-vbmistry" );
        
        var token2 = await source2.GetToken();

        Assert.That(token2,Is.Not.WhiteSpace);
        
    }

    [Test]
    public async Task HttpClient_Should_Make_Authenticated_Call()
    {

        await using var scope = TheContainer.BeginLifetimeScope();

        var factory = scope.Resolve<IHttpClientFactory>();
        
        using var client = factory.CreateClient("MeshtekUat");

        var response = await client.GetAsync("/v1/mission");
        
        var json = await response.Content.ReadAsStringAsync();
        
        Assert.That(json,Is.Not.Null);
        Assert.That(json,Is.Not.Empty);

    }    
    
    
    
    [Test]
    public async Task AccessTokenSource_Should_Return_New_Token_After_Renewal()
    {

        await using var scope = TheContainer.BeginLifetimeScope();
        var source = scope.ResolveNamed<ITokenSource>( "qbo-fortium-us-sandbox" );
        
        var token = await source.GetToken();

        Assert.That(token,Is.Not.WhiteSpace);
        
        await source.CheckForRenewal(true);

        var token2 = await source.GetToken();

        Assert.That(token2,Is.Not.WhiteSpace);
        
        Assert.That(token,Is.Not.EqualTo(token2));
        
    }    
    
    
    private class TheModule : ServiceModule
    {
        
        protected override void Load()
        {

            Builder.AddCorrelation();

            Builder.UseAws("kampilan")
                .AddDynamodbClient("fabrica-");

            

/*
          var qboGrant = new CredentialGrant
            {
                Kind = CredentialGrantKind.RefreshToken,
                Name = "QboSandboxUs",
                MetaEndpoint = QboMetaEndpoint,
                ClientId = QboClientId,
                ClientSecret = QboClientSecret,
                RefreshToken = QboRefreshToken
            };

            Builder.AddMemoryCredentialGrantRepository([qboGrant]);
*/

            // ********************************************************************
            Builder.AddOidcTokenProducer();
            Builder.AddDynCredentialGrantRepository();            

            Builder.AddTokenSource("qbo-fortium-us-sandbox");
            Builder.AddTokenSource("meshtek-prod-ro-vbmistry");

            
            // ********************************************************************            
            Services.ConfigureHttpClientDefaults(c =>
            {
                c.AddStandardResilienceHandler();
            });

            Builder.AddHttpClientWithGrant( "Qbo", "qbo-fortium-us-sandbox");

            Builder.AddHttpClientWithGrant( "MeshtekUat", "meshtek-prod-ro-vbmistry", cfg =>
            {
                cfg.BaseAddress = new Uri("https://uat-api.ilumisolutions.net");
            } );
            

        }
        
        
    }    
    
    
}

