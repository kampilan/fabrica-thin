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

using System.Drawing;
using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;
using MongoDB.Driver;


namespace Fabrica.Watch.Mongo.Switches;

public class MongoSwitchSource: SwitchSource
{


    public string WatchServerUrl { get; set; } = "";
    public MongoSwitchSource WithWatchServerUrl( string uri )
    {

        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));

        WatchServerUrl = uri;
        return this;
    }


    public string DomainName { get; set; } = "";
    public MongoSwitchSource WithDomainName( string domainName )
    {

        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(domainName));

        DomainName = domainName;
        return this;
    }



    private IMongoCollection<DomainEntity> DomainCollection { get; set; } = null!;
    private IMongoCollection<SwitchEntity> SwitchCollection { get; set; } = null!;

    private ConsoleEventSink DebugSink { get; } = new ();


    private bool Started { get; set; }
    public override async Task Start()
    {

        try
        {

            if (Started)
                return;
            
            var client   = new MongoClient(WatchServerUrl);
            var database = client.GetDatabase("fabrica_watch");

            DomainCollection = database.GetCollection<DomainEntity>("domains");
            SwitchCollection = database.GetCollection<SwitchEntity>("switches");

            await UpdateAsync();

            Started = true;

            await base.Start();

        }
        catch( Exception cause )
        {
            var logger  = DebugSink.GetLogger<MongoSwitchSource>();

            logger.Error(cause, "Failed to start");
        }

    }


    public override async Task UpdateAsync( CancellationToken cancellationToken=default )
    {

        if( cancellationToken.IsCancellationRequested )
            return;
        
        try
        {

            var domCursor = DomainCollection.Find(d => d.Name == DomainName);
            var domain    = await domCursor.SingleOrDefaultAsync(cancellationToken: cancellationToken);

            var switchCursor = SwitchCollection.Find(s=>s.DomainUid == domain.Uid);
            var switchList   = await switchCursor.ToListAsync(cancellationToken: cancellationToken);


            var switches = new List<SwitchDef>();

            foreach( var se in switchList )
            {


                if ( !(Enum.TryParse( se.Level, true, out Level lv)) )
                    lv = Level.Warning;


                var color = Color.White;
                try
                {
                    color = Color.FromName(se.Color ?? "White");
                }
                catch
                {
                    // ignore
                }


                var sw = new SwitchDef
                {
                    Pattern      = se.Pattern ?? "",
                    Tag          = se.Tag ?? "",
                    FilterType   = se.FilterType ?? "",
                    FilterTarget = se.FilterTarget ?? "",
                    Level        = lv,
                    Color        = color
                };

                switches.Add(sw);

            }



            Update( switches );



        }
        catch (Exception cause)
        {
            var logger = DebugSink.GetLogger<MongoSwitchSource>();
            logger.Error(cause, "Failed to fetch switches");
        }


    }


}