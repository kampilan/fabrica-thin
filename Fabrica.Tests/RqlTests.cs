using Fabrica.Endpoints;
using Fabrica.Models;
using Fabrica.Rql.Parser;
using Fabrica.Utilities.Types;
using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace Fabrica.Tests;

[TestFixture]
public class RqlTests
{

    public void Test0400_0100_Should_Parse()
    {

        var tree = RqlLanguageParser.ToCriteria("(sw(Name,'Fabrica.Watch.Development'))");


    }
    
    [Test]
    public void Test0400_0200_Should_FindTypes()
    {

        var source = new ModuleSource();
        source.AddTypes(GetType().Assembly);
        
        var types = source.GetTypes();

        var x = types.First();                                                                    
        
        var y = Activator.CreateInstance(x);
        
        
    }
    
}

public class ModuleSource : TypeSource
{
    protected override Func<Type, bool> GetPredicate() => t=>t.IsAssignableTo(typeof(IEndpointModule)); 

}

public static class RetrieveUser
{

    public class Request: IRequest<Response<Person>>
    {
        public string Uid { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Request,Response<Person>> 
    {

        public async Task<Response<Person>> Handle(Request request, CancellationToken cancellationToken)
        {

            await Task.Delay(1000, cancellationToken);
            var person = new Person();
            
            return person;
            
        }
        
    }

    public class Endpoint : IEndpointModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {

        }
    }
    
    
}