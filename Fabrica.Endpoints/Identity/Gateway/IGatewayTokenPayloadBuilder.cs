using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Identity.Gateway
{


    public interface IGatewayTokenPayloadBuilder
    {

        IClaimSet Build( HttpContext context );

        IClaimSet Build(IEnumerable<Claim> claims );


    }


}
