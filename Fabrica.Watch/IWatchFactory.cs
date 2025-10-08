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

using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;

namespace Fabrica.Watch;


public interface IWatchFactory
{

    ISwitchSource Switches { get; }
    
    Task StartAsync();
    Task StopAsync();

    void Accept( LogEvent logEvent );
    
    Task FlushEventsAsync( TimeSpan waitInterval= default, CancellationToken cancellationToken = default); 
    Task UpdateSwitchesAsync( CancellationToken cancellationToken = default );
    
    bool IsTraceEnabled( string category );
    bool IsDebugEnabled( string category );
    
    bool IsTraceEnabled( Type type );
    bool IsDebugEnabled( Type type );

    bool IsTraceEnabled<T>();
    bool IsDebugEnabled<T>();
    
    ILogger GetLogger( string category, bool retroOn = true );
    ILogger GetLogger<T>( bool retroOn = true );
    ILogger GetLogger( Type type, bool retroOn = true );

    ILogger GetLogger( ref LoggerRequest request, bool retroOn = true );

    LogEvent AcquireLogEvent();


}