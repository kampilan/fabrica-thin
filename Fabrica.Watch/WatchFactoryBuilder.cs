/*
The MIT License (MIT)

Copyright (c) 2025 Pond Hawk Technologies Inc.

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
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMethodReturnValue.Global

namespace Fabrica.Watch;

public class WatchFactoryBuilder
{


    public static WatchFactoryBuilder Create()
    {
        return new WatchFactoryBuilder();
    }

    public bool UseAutoUpdater { get; set; }
    
    public int InitialPoolSize { get; set; } = 50;
    public int MaxPoolSize { get; set; } = 500;

    public int ChannelCapacity { get; set; } = 1000;
    public int BatchSize { get; set; } = 500;

    public ISwitchSource Source { get; set; } = new SwitchSource();

    public IEventSinkProvider Sink { get; set; } = new ConsoleEventSink();

    private bool Quiet { get; set; }

    public WatchFactoryBuilder UseQuiet()
    {
        Quiet = true;
        return this;
    }
   

    public async Task BuildAsync()
    {

        if( Quiet )
        {
            var factory = new QuietLoggerFactory();
            await WatchFactoryLocator.SetFactory(factory);
        }
        else
        {
                        
            var config = new WatchFactoryConfig
            {

                Quiet = Quiet,
                UseAutoUpdate = UseAutoUpdater,

                InitialPoolSize = InitialPoolSize,
                MaxPoolSize = MaxPoolSize,

                ChannelCapacity = ChannelCapacity,
                BatchSize = BatchSize,

                Switches = Source,
                Sink = Sink

            };

            var factory = new WatchFactory( config );

            await WatchFactoryLocator.SetFactory(factory);            
            
        }        
        

    }



}