using System.ComponentModel;
using Fabrica.Patch.Builder;
using Fabrica.Patch.Models;
using Fabrica.Utilities.Types;

namespace Fabrica.Tests;


public class Person : BaseMutableModel<Person>, INotifyPropertyChanged
{

    public Person( bool added = false )
    {
        if (added)
            Added();
    }



    public Person()
    {
        _addresses = new DependentObservable<Address>(this, nameof(Addresses), [] );
    }

    public override string GetUid()
    {
        return Uid;
    }

    public string Uid { get; set; } = Ulid.NewUlid();

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public School HighSchool { get; set; } = null!;

    private DependentObservable<Address> _addresses = null!;

    public ICollection<Address> Addresses
    {
        get => _addresses;
        set => _addresses = new DependentObservable<Address>(this, nameof(Addresses), value);
    }

}