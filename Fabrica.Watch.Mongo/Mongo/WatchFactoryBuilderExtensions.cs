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


// ReSharper disable UnusedMember.Global

using Fabrica.Watch.Mongo.Sink;
using Fabrica.Watch.Mongo.Switches;
using JetBrains.Annotations;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global


namespace Fabrica.Watch.Mongo;

[UsedImplicitly]
public static class WatchFactoryBuilderExtensions
{


    public static WatchFactoryBuilder UseMongo( this WatchFactoryBuilder builder, IWatchMongoModule module )
    {


        builder.UseMongoSink( module );
        builder.UseMongoSwitchSource( module );

        return builder;

    }


    public static WatchFactoryBuilder UseMongo( this WatchFactoryBuilder builder, string serverUri, string domainName )
    {

        builder.UseMongoSink( serverUri, domainName );
        builder.UseMongoSwitchSource( serverUri, domainName );

        return builder;

    }



    public static WatchFactoryBuilder UseMongoSink( this WatchFactoryBuilder builder, IWatchMongoModule module )
    {


        var sink = new MongoEventSink
        {
            ServerUri  = module.WatchEventStoreUri,
            DomainName = module.WatchDomainName
        };

        builder.Sink = sink;


        return builder;

    }


    public static WatchFactoryBuilder UseMongoSink(this WatchFactoryBuilder builder, string serverUri, string domainName )
    {

        var sink = new MongoEventSink
        {
            ServerUri  = serverUri,
            DomainName = domainName
        };

        builder.Sink = sink;


        return builder;

    }


    public static WatchFactoryBuilder UseMongoSwitchSource( this WatchFactoryBuilder builder, IWatchMongoModule module )
    {

        var source = new MongoSwitchSource
        {
            DomainName     = module.WatchDomainName,
            WatchServerUrl = module.WatchEventStoreUri,
        };


        builder.Source = source;

        return builder;

    }



    public static WatchFactoryBuilder UseMongoSwitchSource( this WatchFactoryBuilder builder, string serverUri, string domainName )
    {

        var source = new MongoSwitchSource
        {
            WatchServerUrl  = serverUri,
            DomainName = domainName
        };

        builder.Source = source;

        return builder;

    }



}