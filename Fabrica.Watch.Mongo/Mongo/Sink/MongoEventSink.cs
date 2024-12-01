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

// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedMember.Local

using Fabrica.Watch.Sink;
using MongoDB.Bson;
using MongoDB.Driver;


namespace Fabrica.Watch.Mongo.Sink;

public class MongoEventSink: IEventSinkProvider
{

    // ReSharper disable once ClassNeverInstantiated.Local
    private class DomainModel
    {


        public ObjectId Id { get; set; }

        public string Uid { get; set; } = "";

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public long NonDebugTimeToLiveSeconds { get; set; }
        public long DebugTimeToLiveSeconds { get; set; }

        public string ServerUri { get; set; } = "";
        public string Database { get; set; } = "";
        public string Collection { get; set; } = "";

    }


    public string ServerUri { get; set; } = "";
    public MongoEventSink WithServerUri( string uri )
    {

        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));

        ServerUri = uri;
        return this;
    }

    public string DomainName { get; set; } = "";
    public MongoEventSink WithDomainName( string domainName )
    {

        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(domainName));

        DomainName = domainName;
        return this;
    }


    public TimeSpan DebugTimeToLive { get; set; } = TimeSpan.FromHours(4);
    public TimeSpan NonDebugTimeToLive { get; set; } = TimeSpan.FromDays(7);


    public Task Start()
    {


        // ***************************************************
        var wc   = new MongoClient( ServerUri );
        var wdb  = wc.GetDatabase("fabrica_watch");
        var wcol = wdb.GetCollection<DomainModel>("domains");



        // ***************************************************
        var cursor = wcol.Find(d => d.Name == DomainName);
        var domain = cursor.FirstOrDefault();

        if( domain == null )
            throw new Exception( $"Could not find Domain using name: {DomainName}" );



        // ***************************************************
        var client    = new MongoClient( domain.ServerUri );
        var database  = client.GetDatabase( domain.Database );

        Collection = database.GetCollection<BsonDocument>(domain.Collection);

        var models = EnsureIndexes();
        if( models.Count > 0 )
            Collection.Indexes.CreateMany(models);

        return Task.CompletedTask;


    }


    protected virtual List<CreateIndexModel<BsonDocument>> EnsureIndexes()
    {

        var list    = new List<CreateIndexModel<BsonDocument>>();
        var builder = new IndexKeysDefinitionBuilder<BsonDocument>();

        list.Add(new CreateIndexModel<BsonDocument>(builder.Ascending(nameof(MongoLogEntity.TimeToLive)), new CreateIndexOptions
        {
            Name        = "Cleanup",
            ExpireAfter = TimeSpan.Zero
        }));

        list.Add(new CreateIndexModel<BsonDocument>(builder.Ascending(nameof(MongoLogEntity.Occurred)), new CreateIndexOptions
        {
            Name = "ByOccurred"
        }));

        list.Add(new CreateIndexModel<BsonDocument>(builder.Ascending(nameof(MongoLogEntity.CorrelationId)), new CreateIndexOptions
        {
            Name = "ByCorrelation"
        }));

        return list;

    }


    private IMongoCollection<BsonDocument> Collection { get; set; } = null!;
    

    private ConsoleEventSink DebugSink { get; } = new();


    public async Task Accept( LogEventBatch batch, CancellationToken ct=default )
    {

        try
        {

            var documents = new List<BsonDocument>();
            foreach (var le in batch.Events)
            {

                var ttl = le.Level <= (int)Level.Debug ? Convert.ToInt64( le.Occurred + DebugTimeToLive.TotalMicroseconds ) : Convert.ToInt64( le.Occurred + NonDebugTimeToLive.TotalMicroseconds );

                var doc = new BsonDocument
                {
                    {"Category", le.Category},
                    {"CorrelationId", le.CorrelationId},
                    {"Tenant", le.Tenant},
                    {"Subject", le.Subject},
                    {"Tag", le.Tag},
                    {"Title", le.Title},
                    {"Level", le.Level},
                    {"Color", le.Color},
                    {"Nesting", le.Nesting},
                    {"Type", le.Type},
                    {"Payload", le.Base64},
                    {"Occurred", le.Occurred},
                    {"TimeToLive", ttl},
                };

                documents.Add(doc);

            }



            await Collection.InsertManyAsync(documents, cancellationToken: ct);

        }
        catch (Exception cause)
        {

            var logger = DebugSink.GetLogger<MongoEventSink>();

            logger.Error(cause, "Failed to accept batch");

        }

    }

    public Task Stop()
    {

        return Task.CompletedTask;

    }


}