/*
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

using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;

namespace Fabrica.Watch;

public class WatchFactoryBuilder
{


    public static WatchFactoryBuilder Create()
    {
        return new WatchFactoryBuilder();
    }

    public int InitialPoolSize { get; set; } = 50;
    public int MaxPoolSize { get; set; } = 500;


    public int BatchSize { get; set; } = 10;
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(50);
    public TimeSpan WaitForStopInterval { get; set; } = TimeSpan.FromSeconds(5);


    public ISwitchSource Source { get; set; } = new SwitchSource();


    private readonly List<IEventSinkProvider> _sinks = [];
    public void AddSink(IEventSinkProvider sink)
    {
        _sinks.Add(sink);
    }



    private bool Quiet { get; set; }

    public WatchFactoryBuilder UseQuiet()
    {
        Quiet = true;
        return this;
    }

    private bool Foreground { get; set; }

    public WatchFactoryBuilder UseForegroundFactory()
    {
        Foreground = true;
        return this;
    }    
    

    public void Build()
    {

        if( Quiet )
        {
            var factory = new QuietLoggerFactory();
            WatchFactoryLocator.SetFactory(factory);
        }
        else if (Foreground)
        {
                        
            var config = new WatchFactoryConfig
            {
                Quiet = Quiet,
                InitialPoolSize = InitialPoolSize,
                MaxPoolSize = MaxPoolSize,
                BatchSize = BatchSize,
                PollingInterval = PollingInterval,
                WaitForStopInterval = WaitForStopInterval,

                Switches = Source,
                Sinks = _sinks

            };

            var factory = new ForegroundWatchFactory(config);

            WatchFactoryLocator.SetFactory(factory);            
            
        }        
        else
        {

            var config = new WatchFactoryConfig
            {
                Quiet = Quiet,
                InitialPoolSize = InitialPoolSize,
                MaxPoolSize = MaxPoolSize,
                BatchSize = BatchSize,
                PollingInterval = PollingInterval,
                WaitForStopInterval = WaitForStopInterval,

                Switches = Source,
                Sinks = _sinks

            };

            var factory = new BackgroundWatchFactory(config);

            WatchFactoryLocator.SetFactory(factory);            
            
        }        

    }


    public IWatchFactory BuildNoSet()
    {

        if (Quiet)
        {
            var factory = new QuietLoggerFactory();
            return factory;
        }
        else
        {

            var config = new WatchFactoryConfig
            {
                Quiet = Quiet,
                InitialPoolSize = InitialPoolSize,
                MaxPoolSize = MaxPoolSize,
                BatchSize = BatchSize,
                PollingInterval = PollingInterval,
                WaitForStopInterval = WaitForStopInterval,

                Switches = Source,
                Sinks = _sinks

            };

            var factory = new BackgroundWatchFactory(config);

            return factory;            
            
        }

        
    }


}