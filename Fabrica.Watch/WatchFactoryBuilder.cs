/*
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
using System.Drawing;

namespace Fabrica.Watch;

public class WatchFactoryBuilder
{


    public static WatchFactoryBuilder Create()
    {
        return new WatchFactoryBuilder();
    }



    public int InitialPoolSize { get; set; } = 50;
    public int MaxPoolSize { get; set; } = 500;


    public CompositeSink Sink { get; } = new ();

    public ISwitchSource Source { get; set; } = new SwitchSource();

    private bool Quiet { get; set; }

    public WatchFactoryBuilder UseQuiet()
    {
        Quiet = true;
        return this;
    }

    
    public WatchFactoryBuilder UseBatching( int batchSize=10, TimeSpan pollingInterval=default )
    {

        Sink.BatchSize = batchSize;

        if ( pollingInterval != default)
            Sink.PollingInterval = pollingInterval;

        return this;

    }





    public void Build<TFactory>() where TFactory : class, IWatchFactory, new()
    {

        var factory = new TFactory();

        factory.Configure( Source, Sink, Quiet );

        WatchFactoryLocator.SetFactory( factory );

    }

    public void Build<TFactory>( Func<TFactory> builder ) where TFactory : class, IWatchFactory
    {

        var factory = builder();

        factory.Configure(Source, Sink, Quiet );

        WatchFactoryLocator.SetFactory( factory );

    }

    public void Build()
    {

        var factory = new WatchFactory(InitialPoolSize, MaxPoolSize);

        factory.Configure( Source, Sink, Quiet );

        WatchFactoryLocator.SetFactory(factory);

    }



    public IWatchFactory BuildNoSet()
    {

        var factory = new WatchFactory(InitialPoolSize);

        factory.Configure(Source, Sink, Quiet);

        return factory;

    }





}