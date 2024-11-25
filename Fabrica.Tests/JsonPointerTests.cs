using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;
using Fabrica.Utilities.Types;
using Json.More;
using Json.Patch;
using Json.Pointer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;

namespace Fabrica.Tests;

public class JsonPointerTests
{

    [Test]
    public void Test_0600_0100_Should_Find_Name()
    {

        var json = new SampleJson().Sample1;
        var node = JsonDocument.Parse(json);

        var ptrId           = JsonPointer.Parse("/Id");
        var ptrName         = JsonPointer.Parse("/Name");
        var ptrType         = JsonPointer.Parse("/Type");
        var ptrDueDays      = JsonPointer.Parse("/DueDays");
        var ptrDiscountDays = JsonPointer.Parse("/DiscountDays");
        var ptrLastUpdate   = JsonPointer.Parse("/MetaData/LastUpdatedTime");


        var id = ptrId.Evaluate(node.RootElement).ToString();
        var name = ptrName.Evaluate(node.RootElement).ToString();
        var type = ptrType.Evaluate(node.RootElement).ToString();
        var dueDays = ptrDueDays.Evaluate(node.RootElement)?.GetInt32() ?? 0;
        var discountDays = ptrDiscountDays.Evaluate(node.RootElement);
        var lastUpdate = ptrLastUpdate.Evaluate(node.RootElement)?.GetDateTime()??DateTime.MinValue;


        var dd = 0;
        if( discountDays is {ValueKind: JsonValueKind.Number} )
        {
            dd = discountDays.Value.GetInt32();
        }
        else if( discountDays is {ValueKind: JsonValueKind.Null} )
        {
            dd = -1;
        }

        Assert.That(name, Is.EqualTo("Net 90") );
        Assert.That(dd, Is.EqualTo(-1));


    }

    [Test]
    public async Task Test_0600_0200_Should_Handle_Parameters()
    {


        var json = new SampleJson().Sample1;
        var node = JsonDocument.Parse(json);

        var ptrId          = JsonPointer.Parse("/Term/Id");
        var ptrActive      = JsonPointer.Parse("/Term/Active");
        var ptrName        = JsonPointer.Parse("/Term/Name");
        var ptrType        = JsonPointer.Parse("/Term/Type");
        var ptrDiscPercent = JsonPointer.Parse("/Term/DiscountPercent");
        var ptrDiscDays    = JsonPointer.Parse("/Term/DiscountDays");
        var ptrDueDays     = JsonPointer.Parse("/Term/DueDays");
        var ptrCreateDate  = JsonPointer.Parse("/Term/MetaData/CreateTime");
        var ptrLastUpdate  = JsonPointer.Parse("/Term/MetaData/LastUpdatedTime");


        var builder = new DbContextOptionsBuilder();
        builder.UseSqlite("data source=c:/temp/test.db");

        var ctx = new SampleDb(builder.Options);
        await ctx.Database.MigrateAsync();


        await using var trx = await ctx.Database.BeginTransactionAsync();        

        var cn = ctx.Database.GetDbConnection();
        var cmd = cn.CreateCommand();
        cmd.CommandText = "INSERT INTO Terms (ExternalId, Active, Name, Type, DueDays, DiscountPercent, DiscountDays, CreateTime, LastUpdatedTime ) VALUES (@ExternalId, @Active, @Name, @Type, @DueDays, @DiscountPercent, @DiscountDays, @CreateTime, @LastUpdatedTime )";

        var externalId = ptrId.Evaluate(node.RootElement).ToString();
        cmd.Parameters.Add(new SqliteParameter("@ExternalId", externalId));

        var active = ptrActive.Evaluate(node.RootElement)?.GetBoolean()??false;
        cmd.Parameters.Add(new SqliteParameter("@Active", active));

        var name = ptrName.Evaluate(node.RootElement).ToString();
        cmd.Parameters.Add(new SqliteParameter("@Name", name));

        var type = ptrType.Evaluate(node.RootElement).ToString();
        cmd.Parameters.Add(new SqliteParameter("@Type", type));

        var dueDays = ptrDueDays.Evaluate(node.RootElement)?.GetInt32()??0;
        cmd.Parameters.Add(new SqliteParameter("@DueDays", dueDays));

        var discountDays = ptrDiscDays.Evaluate(node.RootElement)?.GetInt32()??0;
        cmd.Parameters.Add(new SqliteParameter("@DiscountDays", discountDays));

        var discountPercent = ptrDiscPercent.Evaluate(node.RootElement)?.GetDecimal()??0m;
        cmd.Parameters.Add(new SqliteParameter("@DiscountPercent", discountPercent));

        var created = ptrCreateDate.Evaluate(node.RootElement)?.GetDateTime() ?? DateTime.MinValue;
        cmd.Parameters.Add(new SqliteParameter("@CreateTime", created));

        var lastUpdate = ptrLastUpdate.Evaluate(node.RootElement)?.GetDateTime() ?? DateTime.MinValue;
        cmd.Parameters.Add(new SqliteParameter("@LastUpdatedTime", lastUpdate));

        var rows = await cmd.ExecuteNonQueryAsync();
        await trx.CommitAsync();


    }

    [Test]
    public async Task Test_0600_0200_Should_Load_From_Xml()
    {

        await using var fs = new FileStream(@"E:\repository\fabrica-qbo-gateway\integration-map.xml", FileMode.Open, FileAccess.Read );

        var integration = IntegrationLoader.Load(fs);
        
        Assert.That(integration, Is.Not.Null);

    }


    [Test]
    public async Task Test_0600_0200_Should_Handle_From_Integration()
    {

        await using var fs = new FileStream(@"E:\repository\fabrica-qbo-gateway\integration-map.xml", FileMode.Open, FileAccess.Read);

        var integration = IntegrationLoader.Load(fs);

        integration.EndpointTemplate = "https://sandbox-quickbooks.api.intuit.com/v3/company/4620816365066815590/{0}?minorversion=73";
        integration.EndpointWithIdTemplate = "https://sandbox-quickbooks.api.intuit.com/v3/company/4620816365066815590/{0}/{1}?minorversion=73";

        integration.LoadSource( new SampleJson().Sample1 );
        

        var db = await BuildDatabase( "data source=c:/temp/test.db", true );


        await using var trx = await db.BeginTransactionAsync();

        var cn = db.GetDbConnection();

        var dataset = integration.Datasets["Term"];


        var insert = dataset.BuildInsert( cn );
        await insert.ExecuteNonQueryAsync();

        var update = dataset.BuildUpdate(cn);
        await update.ExecuteNonQueryAsync();

        var delete = dataset.BuildDelete(cn);
        await delete.ExecuteNonQueryAsync();

        Console.WriteLine(dataset.Endpoint);
        Console.WriteLine(dataset.EndpointWithId);
        Console.WriteLine(dataset.ExternalId);

        await trx.CommitAsync();
        return;


        // ReSharper disable once IdentifierTypo
        async Task<DatabaseFacade> BuildDatabase( string dbConnection, bool runMigrations )
        {

            var builder = new DbContextOptionsBuilder();
            builder.UseSqlite(dbConnection);

            var ctx = new SampleDb(builder.Options);
            var dbf = ctx.Database;

            if( runMigrations )
                await dbf.MigrateAsync();

            return dbf;

        }



    }



    [Test]
    public void Test_0600_0300_Should_Patch_JSON_Sample2()
    {
        var pId = JsonPointer.Parse("/Id");
        var pName = JsonPointer.Parse("/Name");
        var pDueDays = JsonPointer.Parse("/DueDays");

        var list = new List<PatchOperation>
        {
            PatchOperation.Replace(pId, "10"),
            PatchOperation.Replace(pName, "Net 30"),
            PatchOperation.Replace(pDueDays, 30)
        };

        var patch = new JsonPatch(list);

        var patch2 = new JsonPatch();

        var node = JsonDocument.Parse(new SampleJson().Sample2);
        var post = patch.Apply(node.RootElement);

        var json = post.GetRawText();


    }


    [Test]
    public void Test_0600_0300_Should_Point_To_JSON_Array()
    {

        var pList = JsonPointer.Parse("/List");

        var node = JsonDocument.Parse(new SampleJson().Sample4);

        var list = pList.Evaluate(node.RootElement);

        Assert.That(list?.ValueKind, Is.EqualTo(JsonValueKind.Array));


        var ops = new List<PatchOperation>
        {
            PatchOperation.Add(JsonPointer.Create("Test"), "10"),
        };

        var patch = new JsonPatch(ops);

        var obj = new JsonObject();
        var post = patch.Apply(obj);

        Console.WriteLine(post.Result?.ToJsonString()??"");

        var pId = JsonPointer.Parse("/Id");
        var pName = JsonPointer.Parse("/Name");
        var pList2 = JsonPointer.Parse("/List/-");


        var ops2 = new List<PatchOperation>
        {
            PatchOperation.Add(pId, "7"),
            PatchOperation.Add(pName, "Net 30"),
            PatchOperation.Add(pList2, post.Result),
        };

        var patch2 = new JsonPatch(ops2);
        var postNode = patch2.Apply(node.RootElement);


        Console.WriteLine(postNode.AsNode()!.ToJsonString());


    }


}


public class SampleJson
{

    public string Sample1 =
        """
        {
          "Term": {
            "SyncToken": "0", 
            "domain": "QBO", 
            "Name": "Net 30", 
            "DiscountPercent": 5, 
            "DiscountDays": 10, 
            "Type": "STANDARD", 
            "sparse": false, 
            "Active": true, 
            "DueDays": 30, 
            "Id": "10", 
            "MetaData": {
              "CreateTime": "2014-09-11T14:41:49-07:00", 
              "LastUpdatedTime": "2014-09-11T14:41:49-07:00"
            }
          }, 
          "time": "2015-07-28T08:52:57.63-07:00"
        }

        """;

    public string Sample2 =
        """
        {
          "SyncToken": "0", 
          "domain": "QBO", 
          "Name": "", 
          "DiscountPercent": 0, 
          "DiscountDays": 0, 
          "Type": "", 
          "sparse": false, 
          "Active": false, 
          "DueDays": 0, 
          "Id": "", 
          "MetaData": {
            "CreateTime": "", 
            "LastUpdatedTime": ""
          }
        }
        """;

    public string Sample3 =
        """
        {
          "SyncToken": "0", 
          "domain": "QBO", 
          "Name": "", 
          "DiscountPercent": 0, 
          "DiscountDays": 0, 
          "Type": "", 
          "sparse": false, 
          "Active": false, 
          "DueDays": 0, 
          "Id": "",
          "List": [
            {"Test": "One"},
            {"Test": "Two"},
            {"Test": "Three"}
          ],
          "List2": [] 

        }
        """;


    public string Sample4 =
        """
        {

          "SyncToken": "0", 
          "domain": "QBO", 
          "sparse": false, 
          "List": []

        }
        """;


}


public class SampleDb : DbContext
{

    public SampleDb()
    {

    }
    
    public SampleDb(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Terms> Terms { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {

        builder.UseSqlite("data source=c:/temp/test.db");

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Terms>()
            .ToTable("Terms")
            .HasKey(x => x.Id);
    }

}




public class Terms
{

    public long Id { get; set; }

    public bool Active { get; set; }


    [MaxLength(25)]
    [DefaultValue("")]
    public string ExternalId { get; set; } = string.Empty;

    [MaxLength(100)]
    [DefaultValue("")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    [DefaultValue("")]
    public string Type { get; set; } = string.Empty;

    [DefaultValue(0)]
    public int DueDays { get; set; }

    [DefaultValue(0)]
    public int DiscountDays { get; set; }

    [DefaultValue(0)]
    public decimal DiscountPercent { get; set; }

    public DateTime CreateTime { get; set; }
    public DateTime LastUpdatedTime { get; set; }

    public long RemoteTimestamp { get; set; }
    public long LocalTimestamp { get; set; }


}


public static class IntegrationLoader
{


    public static Integration Load( string xml )
    {

        using var stream = new MemoryStream();
        using var writer = new StreamWriter( stream );
        writer.Write(xml);

        stream.Seek( 0, SeekOrigin.Begin );

        using var reader = XmlReader.Create( stream );

        return Load(reader);
        
    }

    public static Integration Load(Stream stream)
    {

        using var reader = XmlReader.Create(stream);

        return Load(reader);
    }


    public static Integration Load( XmlReader reader )
    {

        
        var integration = new Integration();
        var xDoc = XDocument.Load(reader);

        if (xDoc.Root is null)
            return integration;


        foreach ( var dsElem in xDoc.Root.Descendants("dataset") )
        {

            var name     = dsElem.Attribute("name")?.Value ?? string.Empty;
            var resource = dsElem.Attribute("resource")?.Value ?? string.Empty;

            var dataset = new IntegrationDataset(integration)
            {
                Name     = name,
                Resource = resource
            };
            
            integration.Datasets.Add(name, dataset );

            dataset.InsertTemplate = dsElem.Element("select-template")?.Value ?? string.Empty;
            dataset.InsertTemplate = dsElem.Element("insert-template")?.Value ?? string.Empty;
            dataset.UpdateTemplate = dsElem.Element("update-template")?.Value ?? string.Empty;
            dataset.DeleteTemplate = dsElem.Element("delete-template")?.Value ?? string.Empty;

            var properties = dsElem.Element("properties");
            if( properties is null || !properties.HasElements )
                continue;

            foreach( var prElem in properties.Descendants("property") )
            {

                var prop = new IntegrationProperty(dataset)
                {
                    Name   = prElem.Attribute("name")?.Value ?? string.Empty,
                    Type   = prElem.Attribute("type")?.Value ?? string.Empty,
                    Marker = prElem.Attribute("marker")?.Value ?? string.Empty,
                    In     = prElem.Attribute("in")?.Value ?? string.Empty,
                    Out    = prElem.Attribute("in")?.Value ?? string.Empty,
                    Id     = prElem.Attribute("id")?.Value == "true"
                };

                prop.InPointer = JsonPointer.Parse( prop.In );
                prop.OutPointer = JsonPointer.Parse( prop.Out );

                dataset.Properties.Add( prop );

            }


        }
        
        return integration;
        
        
    }


}





public class Integration
{

    public string EndpointTemplate { get; set; } = string.Empty;
    public string EndpointWithIdTemplate { get; set; } = string.Empty;

    public Dictionary<string, IntegrationDataset> Datasets { get; } = [];

    public JsonElement Root { get; private set; }

    public void LoadSource( string json )
    {
        var doc = JsonDocument.Parse(json);
        Root = doc.RootElement;
    }

}


public class IntegrationDataset(Integration parent)
{

    public string Name { get; set; } = string.Empty;

    public string Resource { get; set; } = string.Empty;

    public string Endpoint => string.Format(parent.EndpointTemplate, Resource);
    public string EndpointWithId => string.Format(parent.EndpointWithIdTemplate, Resource, ExternalId);

    public string ExternalId { get; set; } = string.Empty;


    public string SelectTemplate { get; set; } = string.Empty;
    public string InsertTemplate { get; set; } = string.Empty;
    public string UpdateTemplate { get; set; } = string.Empty;
    public string DeleteTemplate { get; set; } = string.Empty;

    public List<IntegrationProperty> Properties { get; } = new();


    public DbCommand BuildInsert( DbConnection cn )
    {

        var cmd = cn.CreateCommand();
        cmd.CommandText = InsertTemplate;

        foreach (var param in Properties)
        {
            var dbp = cmd.CreateParameter();
            param.Populate( parent.Root, dbp );
            cmd.Parameters.Add(dbp);
        }

        var rts = cmd.CreateParameter();
        rts.ParameterName = "@RemoteTimestamp";
        rts.Value = DateTimeHelpers.ToTimestampMilli();
        cmd.Parameters.Add(rts);

        return cmd;

    }


    public DbCommand BuildUpdate( DbConnection cn )
    {

        var cmd = cn.CreateCommand();
        cmd.CommandText = UpdateTemplate;

        foreach (var param in Properties)
        {
            var dbp = cmd.CreateParameter();
            param.Populate( parent.Root, dbp );
            cmd.Parameters.Add(dbp);
        }

        var rts = cmd.CreateParameter();
        rts.ParameterName = "@RemoteTimestamp";
        rts.Value = DateTimeHelpers.ToTimestampMilli();
        cmd.Parameters.Add(rts);

        return cmd;

    }

    public DbCommand BuildDelete( DbConnection cn )
    {

        var cmd = cn.CreateCommand();
        cmd.CommandText = DeleteTemplate;

        foreach( var param in Properties )
        {
            var dbp = cmd.CreateParameter();
            param.Populate( parent.Root, dbp );
            cmd.Parameters.Add(dbp);
        }


        return cmd;

    }


}


public class IntegrationProperty( IntegrationDataset parent )
{

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Marker { get; set; } = string.Empty;

    public string In { get; set; } = string.Empty;
    public string Out { get; set; } = string.Empty;

    public bool Id { get; set; }
    
    public JsonPointer InPointer { get; set; } = null!;
    public JsonPointer OutPointer { get; set; } = null!;

    public object? ToObject( JsonElement? je )
    {
        object? value = Type switch
        {
            "String"   => je?.GetString(),
            "Byte"     => je?.GetByte(),
            "Int16"    => je?.GetInt16(),
            "Int32"    => je?.GetInt32(),
            "Int64"    => je?.GetInt64(),
            "UInt16"   => je?.GetUInt16(),
            "UInt32"   => je?.GetUInt32(),
            "UInt64"   => je?.GetUInt64(),
            "Decimal"  => je?.GetDecimal(),
            "Double"   => je?.GetDouble(),
            "Single"   => je?.GetSingle(),
            "Boolean"  => je?.GetBoolean(),
            "DateTime" => je?.GetDateTime(),
            _ => null
        };

        return value;
    }

    public void Populate( JsonElement source, DbParameter dbp )
    {

        var je = InPointer.Evaluate(source);

        var value = ToObject(je);   

        dbp.ParameterName = Marker;
        dbp.Value = value;

        if( Id )
            parent.ExternalId = value?.ToString()??"";

    }


}