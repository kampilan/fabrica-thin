using System.Diagnostics;
using Fabrica.Utilities.Types;
using MemoryPack;
using NUnit.Framework;
using MemoryPack.Compression;

namespace Fabrica.Tests;



[MemoryPackable]
public partial class EnvelopeM
{
    public List<LogEntityM> Events { get; set; } = [];

}

[MemoryPackable]
public partial class LogEntityM
{

    public string Uid { get; set; } = Ulid.NewUlid();

    public string? Category { get; set; }
    public string? CorrelationId { get; set; }

    public string? Title { get; set; }

    public string? Tenant { get; set; }
    public string? Subject { get; set; }
    public string? Tag { get; set; }

    public int Level { get; set; }
    public int Color { get; set; }
    public int Nesting { get; set; }

    public int Type { get; set; }
    public string? Payload { get; set; }

    public DateTime Occurred { get; set; }
    public DateTime TimeToLive { get; set; }


}





public class MemoryPackTests
{


    [Test]
    public void Test_0100_0100_Should_Roundtrip()
    {

        var original = new EnvelopeM();

        for (var i = 0; i < 10000; i++)
        {

            var message = new LogEntityM
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

        using var compressor = new BrotliCompressor();

        var sw = Stopwatch.GetTimestamp();
        

        MemoryPackSerializer.Serialize(compressor,original);

        var comp = compressor.ToArray();


        using var decompressor = new BrotliDecompressor();

        var bytes = decompressor.Decompress(comp);

        var copy = MemoryPackSerializer.Deserialize<EnvelopeM>(bytes);

        var diff = Stopwatch.GetTimestamp() - sw;
        var lapsed = TimeSpan.FromTicks(diff).TotalMilliseconds;

        Console.WriteLine($"Compressed: {comp.Length} bytes");
        Console.WriteLine($"Bytes: {bytes.Length} bytes");

        Console.WriteLine($"Lapsed: {lapsed} ms");


    }







}




