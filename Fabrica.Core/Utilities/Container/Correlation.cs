﻿/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Security.Claims;
using System.Security.Principal;
using Fabrica.Identity;
using Fabrica.Utilities.Types;
using Fabrica.Watch;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Utilities.Container;

public class Correlation: ICorrelation
{

    private static readonly NullUser TheNullUser = new ();


    public string Uid { get; init; } = Ulid.NewUlid().ToString();

    public string CallerGatewayToken { get; set; } = string.Empty;

    public string CorrelationId { get; }
    public string Tenant { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;    
    
    public IPrincipal Caller { get; set; } = TheNullUser;

    public bool Debug { get; set; }

    public Level Level { get; set; } = Level.Debug;


    public void PopulateCaller(IClaimSet claimSet)
    {

        var ci = new FabricaIdentity(claimSet);
        var principal = new ClaimsPrincipal(ci);

        Subject = principal.GetName();
        
        Caller = principal;
        
    }

    public Correlation Clone()
    {

        var c = new Correlation
        {
            Uid = Uid,
            Caller = Caller,

            CallerGatewayToken = CallerGatewayToken,

            Debug  = Debug,
            Level  = Level,
            Tenant = Tenant

        };

        return c;

    }

}

public class NullUser : IPrincipal, IIdentity
{

    public bool IsInRole(string role)
    {
        return false;
    }

    public IIdentity Identity => this;
    public string AuthenticationType => "None";
    public bool IsAuthenticated => false;
    public string Name => "";

}