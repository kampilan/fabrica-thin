﻿/*
The MIT License (MIT)

Copyright (c) 2024 Pond Hawk Technologies Inc.

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


using Fabrica.Utilities.Hosting;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Fabrica.App.One.Hosting;

public class MissionContext : IMissionContext
{


    public string MissionName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;

    public string ApplianceId { get; set; } = string.Empty;
    public string ApplianceName { get; set; } = string.Empty;
    public string ApplianceBuild { get; set; } = string.Empty;
    public DateTime ApplianceBuildDate { get; set; } = DateTime.MinValue;
    public string ApplianceRoot { get; set; } = "";
    public DateTime ApplianceStartTime { get; set; } = DateTime.MinValue;

    public Dictionary<string, string> ServiceEndpoints { get; set; } = new();


}


