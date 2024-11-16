using System.Diagnostics;
using System.Runtime.Serialization;
using Fabrica.Utilities.Types;
using MemoryPack;
using MessagePack;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Fabrica.Tests;


[DataContract]
public class Envelope
{
    [DataMember(Order = 1)]
    public List<LogEntity> Events { get; set; } = [];

}

[DataContract]
public class LogEntity
{

    [DataMember(Order = 1)]
    public string Id { get; set; } = Ulid.NewUlid();

    [DataMember(Order = 2)]
    public string? Category { get; set; }
    [DataMember(Order = 3)]
    public string? CorrelationId { get; set; }

    [DataMember(Order = 4)]
    public string? Title { get; set; }

    [DataMember(Order = 5)]
    public string? Tenant { get; set; }
    [DataMember(Order = 6)]
    public string? Subject { get; set; }
    [DataMember(Order = 7)]
    public string? Tag { get; set; }

    [DataMember(Order = 8)]
    public int Level { get; set; }
    [DataMember(Order = 9)]
    public int Color { get; set; }
    [DataMember(Order = 10)]
    public int Nesting { get; set; }

    [DataMember(Order = 11)]
    public int Type { get; set; }
    [DataMember(Order = 12)]
    public string? Payload { get; set; }

    [DataMember(Order = 13)]
    public DateTime Occurred { get; set; }
    [DataMember(Order = 14)]
    public DateTime TimeToLive { get; set; }


}




public class MessagePackTests
{

    [Test]
    public void Test_0100_0100_Should_Roundtrip()
    {

        var original = new Envelope();

        for (var i = 0; i < 1000; i++)
        {

            var message = new LogEntity
            {
                Category = "Test",
                CorrelationId = "123",
                Title = "Test",
                Tenant = "Test",
                Subject = "Test",
                Tag = "Test",
                Level = 1,
                Color = 1,
                Nesting = 1,
                Type = 1,
                Payload = "Test",
                Occurred = DateTime.Now,
                TimeToLive = DateTime.Now
            };

            original.Events.Add(message);

        }


        var sw = Stopwatch.GetTimestamp();

        var bytes = MessagePackSerializer.Serialize(original);

        var copy = MessagePackSerializer.Deserialize<Envelope>(bytes);

        var diff = Stopwatch.GetTimestamp() - sw;
        var lapsed = TimeSpan.FromTicks(diff).TotalMilliseconds;

        Console.WriteLine($"Lapsed: {lapsed} ms");




    }


}