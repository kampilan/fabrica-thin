﻿using System.Text.Json;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Utilities.Text.Json;

public class PascalJsonNamingPolicy : JsonNamingPolicy
{

    public override string ConvertName( string name )
    {
        return name;
    }

}