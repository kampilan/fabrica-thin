using System.Text.Json;
using Autofac;
using Fabrica.Patch.Builder;
using NUnit.Framework;

namespace Fabrica.Tests;

public class PatchTests
{

    [OneTimeSetUp]
    public void OneTimeSetup()
    {

        var builder = new ContainerBuilder();

        builder.RegisterModule(new PatchTestModule());

        TheRoot = builder.Build();


    }

    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        TheRoot.Dispose();
    }


    private IContainer TheRoot { get; set; } = null!;


    [Test]
    public void Should_Track_Changes()
    {

        var person = new Person();

        person.FirstName = "James";
        person.LastName = "Moring";

        Assert.That(person.IsModified);


    }


    [Test]
    public void Should_Not_Have_Changes_After_Deserialization()
    {

        var json = """
                   {
                        "FirstName": "James",
                        "LastName": "Moring"
                   }
                   """;


        var person = JsonSerializer.Deserialize<Person>(json);


        Assert.That(person, Is.Not.Null);
        Assert.That(person!.IsModified, Is.False);

        person.FirstName = "James";
        person.LastName = "Moring";

        Assert.That(person.IsModified, Is.False);


        person.FirstName = "Wilma";
        person.LastName = "Laluna";

        Assert.That(person.IsModified, Is.True);


    }


    [Test]
    public void Should_Produce_PatchSet_With_Changes()
    {

        var person = new Person();
        person.SuspendTracking(p =>
        {
            p.FirstName = "James";
            p.LastName = "Moring";

        });
        

        Assert.That(person.IsModified, Is.False);


        person.FirstName = "Wilma";

        Assert.That(person.IsModified, Is.True);


        var set = PatchSet.Create(person);

        Assert.That(set, Is.Not.Null);
        Assert.That(set.GetPatches(), Is.Not.Empty);

        var json = set.ToJson();

        Assert.That(json, Is.Not.Empty);


    }


    [Test]
    public void Should_Track_Dependent_Changes_And_Create_Patch()
    {

        var highSchool = new School {Name = "Maine-Endwell High School"};
        var elemSchool = new School { Name = "Homer Brink Elementary" };

        var person = new Person();
        person.SuspendTracking(p =>
        {
            p.FirstName = "James";
            p.LastName = "Moring";

            p.HighSchool = highSchool;

            var a1 = new Address();
                a1.SuspendTracking(a =>
                {
                    a.Line1 = "618 Valley View Dr";
                    a.Line2 = "";
                    a.City = "Endwell";
                    a.State = "NY";
                    a.Zip = "13760";
                });

            p.Addresses.Add(a1);

            var a2 = new Address();
            a1.SuspendTracking(a =>
            {
                a.Line1 = "10161 Vancouver Rd";
                a.Line2 = "";
                a.City = "Spring Hill";
                a.State = "FL";
                a.Zip = "34608";
            });

            p.Addresses.Add(a2);


        });


        Assert.That(person.IsModified, Is.False);

        var first = person.Addresses.First();
        first.Line2 = "Suite 200";

        person.FirstName = "Wilma";

        person.HighSchool = elemSchool;

        Assert.That(person.IsModified, Is.True);


        var set = PatchSet.Create(person);

        Assert.That(set, Is.Not.Null);
        Assert.That(set.GetPatches(), Is.Not.Empty);
        Assert.That(set.GetPatches().Count(), Is.EqualTo(2));


        var json = set.ToJson();

        Assert.That(json, Is.Not.Empty);

        person.Addresses.Add(new Address(true) { Line1 = "1708 Fox Run Dr", Line2 = "Suite 300", City = "Plainsboro", State="NJ", Zip="08609"});


        var set2 = PatchSet.Create(person);

        Assert.That(set2, Is.Not.Null);
        Assert.That(set2.GetPatches(), Is.Not.Empty);
        Assert.That(set2.GetPatches().Count(), Is.EqualTo(3));
        Assert.That(set2.GetPatches().Count(p => p.Verb == PatchVerb.Update), Is.EqualTo(2));
        Assert.That(set2.GetPatches().Count(p => p.Verb == PatchVerb.Create), Is.EqualTo(1));



        var json2 = set2.ToJson();

        Assert.That(json2, Is.Not.Empty);



    }






}