using System.Collections.ObjectModel;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Persistence.Entities;

public class ObservableDependents<TMember>: ObservableCollection<TMember> where TMember : class, IDependentEntity
{

    public ObservableDependents( IEntity owner, string propertyName )
    {
        AttachOwner(owner, propertyName);
    }


    public ObservableDependents( IEntity owner, string propertyName, IEnumerable<TMember> members )
    {

        AttachOwner(owner, propertyName);

        foreach (var mem in members)
            Add(mem);

    }

    public IEntity Owner { get; private set; } = null!;
    public string PropertyName { get; private set; } = string.Empty;

    public void AttachOwner(IEntity owner, string property)
    {
        Owner = owner;
        PropertyName = property;
    }


    private HashSet<TMember> Guard { get; } = new ();


    #region Observable overrides

    protected override void InsertItem(int index, TMember item)
    {

        if( !Guard.Add(item) )
            return;

        item.SetParent(Owner);

        base.InsertItem(index, item);

    }

    protected override void SetItem(int index, TMember item)
    {

        if( !Guard.Add(item) )
            return;

        item.SetParent(Owner);

        base.SetItem(index, item);

    }

    protected override void ClearItems()
    {

        foreach (var item in this)
        {

            if (!Guard.Remove(item))
                continue;

            item.SetParent(null);


        }

        base.ClearItems();

    }

    protected override void RemoveItem(int index)
    {

        var item = this[index];
        if( !Guard.Remove(item) )
            return;


        item.SetParent(null);


        base.RemoveItem(index);

    }

    #endregion




}




