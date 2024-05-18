using System.ComponentModel;
using Fabrica.Patch.Models;
using Fabrica.Utilities.Types;

namespace Fabrica.Tests;

public class Address: BaseMutableModel<Address>, IDependentModel, INotifyPropertyChanged
{
    public Address(bool added=false)
    {
        if(added)
            Added();
    }

    public override string GetUid()
    {
        return Uid;
    }


    public string Uid { get; set; } = Ulid.NewUlid();

    public Person Parent { get; set; } = null!;
    public void SetParent(object? parent)
    {
        if (parent is Person person)
            Parent = person;
    }


    public string Line1 { get; set; } = string.Empty;
    public string Line2 { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;


}