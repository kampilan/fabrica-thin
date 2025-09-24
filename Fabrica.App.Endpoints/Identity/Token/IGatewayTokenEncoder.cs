using Fabrica.Identity;

namespace Fabrica.App.Identity.Token;

public interface IGatewayTokenEncoder
{

    string Encode( IClaimSet claims );
    IClaimSet Decode( string authType, string token, bool validate=true );


}