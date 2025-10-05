using System.Text.Json;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMember.Global

namespace Fabrica.App.Endpoints.Json;

public static class JsonConfigAutofacExtensions
{

    public static ContainerBuilder ConfigureJsonSerializerOptions(this ContainerBuilder builder, Action<JsonSerializerOptions> optBuilder )
    {

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        optBuilder(options);


        var services = new ServiceCollection();
        services.Configure<JsonOptions>(o =>
        {

            o.SerializerOptions.PropertyNamingPolicy        = options.PropertyNamingPolicy;
            o.SerializerOptions.DefaultIgnoreCondition      = options.DefaultIgnoreCondition;
            o.SerializerOptions.DictionaryKeyPolicy         = options.DictionaryKeyPolicy;
            o.SerializerOptions.ReferenceHandler            = options.ReferenceHandler;
            o.SerializerOptions.PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive;
            o.SerializerOptions.UnmappedMemberHandling      = options.UnmappedMemberHandling;
            o.SerializerOptions.WriteIndented               = options.WriteIndented;
            o.SerializerOptions.TypeInfoResolver            = options.TypeInfoResolver;
            
            foreach( var converter in options.Converters )
                o.SerializerOptions.Converters.Add(converter);
            
        });

        builder.Populate(services);

        builder.RegisterInstance(options)
            .AsSelf()
            .SingleInstance();


        return builder;
    }

}