
// ReSharper disable UnusedMember.Global

namespace Fabrica.Identity;

public class AnonymousClaimsIdentity() : FabricaIdentity( new ClaimSetModel {Subject = "Anonymous"} );

