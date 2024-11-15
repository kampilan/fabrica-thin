using System.Security.Cryptography;
using Fabrica.Utilities.Queue;
using NUnit.Framework;

namespace Fabrica.Tests;

[TestFixture]
public class QueueMessageTests
{


    [Test]
    public void Test_0002_0100_WorkQueueMessage_Should_Save_Valid_Xml()
    {

        var body = new Person
        {
            FirstName = "James",
            LastName = "Moring",
            HighSchool = new School {Name = "Maine-Endwell"}
        };

        var wq = WorkQueueMessage.Create()
            .WithTopic("test-001")
            .WithClaims("1234567890", "automation", "Automation", ["Automation"])
            .WithJson(body);

        var xml = wq.Save();


        var message = WorkQueueMessage.Load(xml);

        var person = message.FromJson<Person>();


    }


    [Test]
    public void Test_0002_0110_WorkQueueMessage_Should_Save_Valid_Xml_And_MatchSignatures()
    {

        var rng = RandomNumberGenerator.Create();
        var key = new byte[64];
        rng.GetBytes(key);

        var signingKey = Convert.ToBase64String(key);


        var body = new Person
        {
            FirstName = "James",
            LastName = "Moring",
            HighSchool = new School { Name = "Maine-Endwell" }
        };

        var wq = WorkQueueMessage.Create()
            .WithTopic("test-001")
            .WithClaims("1234567890", "automation", "Automation", ["Automation"])
            .WithJson(body);

        var (xml,hash) = wq.Save(signingKey);


        var message = WorkQueueMessage.Load(signingKey, xml, hash);

        var person = message.FromJson<Person>();


    }



    [Test]
    public void Test_0002_0200_HubQueueMessage_Should_Save_Valid_Xml()
    {

        var body = new Person
        {
            FirstName = "James",
            LastName = "Moring",
            HighSchool = new School { Name = "Maine-Endwell" }
        };

        var wq = HubQueueMessage.Create()
            .WithTenant("test-001")
            .WithEnvironment("uat")
            .WithTopic("fixed")
            .WithClaims( "1234567890", "automation", "Automation", ["Automation"] )
            .WithJson(body);

        var xml = wq.Save();


        var message = HubQueueMessage.Load(xml);

        var person = message.FromJson<Person>();


    }


    [Test]
    public void Test_0002_0210_HubQueueMessage_Should_Save_Valid_Xml_And_MatchSignatures()
    {

        var rng = RandomNumberGenerator.Create();
        var key = new byte[64];
        rng.GetBytes(key);

        var signingKey = Convert.ToBase64String(key);


        var body = new Person
        {
            FirstName = "James",
            LastName = "Moring",
            HighSchool = new School { Name = "Maine-Endwell" }
        };

        var wq = HubQueueMessage.Create()
            .WithTenant("test-001")
            .WithEnvironment("uat")
            .WithTopic("fixed")
            .WithClaims("1234567890", "automation", "Automation", ["Automation"])
            .WithJson(body);

        var (xml,hash) = wq.Save(signingKey);


        var message = HubQueueMessage.Load(signingKey, xml, hash);

        var person = message.FromJson<Person>();


    }







}