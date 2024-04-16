using Microsoft.AspNetCore.Routing;

namespace Fabrica.Endpoints;


public interface IEndpointModule
{

    string BasePath => string.Empty;

    void Configure( RouteGroupBuilder group )
    {
    }

    void AddRoutes( IEndpointRouteBuilder app );

}


