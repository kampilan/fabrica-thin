﻿namespace Fabrica.App.Identity.Gateway;

public static class IdentityConstants
{

    public static string IdentityHeaderName => "X-Gateway-Identity";
    public static string TokenHeaderName => "X-Gateway-Identity-Token";
        
    public static string Scheme => "Fabrica.GatewayToken";
    public static string Policy => "Fabrica.GatewayToken";

    public static string PublicPolicyName => "AllowPublic";

    public static string AdminPolicyName => "RequiresAdminRole";
    public static string AdminRoleName => "admin";
    

}