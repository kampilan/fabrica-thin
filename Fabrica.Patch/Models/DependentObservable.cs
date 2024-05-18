using System.Collections.ObjectModel;
using System.ComponentModel;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Patch.Models;

public sealed class DependentObservable<TMember>: ObservableCollection<TMember>, IDependentCollection where TMember : class, IDependentModel, IEditableObject, INotifyPropertyChanged
{


    public DependentObservable( IMutableModel owner, string propertyName )
    {
        AttachOwner(owner, propertyName);
    }


    public DependentObservable( IMutableModel owner, string propertyName, IEnumerable<TMember> members )
    {

        AttachOwner(owner,propertyName);

        foreach (var mem in members)
            Add(mem);

    }

    public IMutableModel Owner { get; private set; } = null!;
    public string PropertyName { get; private set; } = string.Empty;

    public void AttachOwner( IMutableModel owner, string property )
    {
        Owner        = owner;
        PropertyName = property;
    }

    private void OnMemberPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Owner.NotifyCollectionChanged(this, (IModel?)sender);
    }


    private HashSet<TMember> Guard { get; } = [];
    private List<TMember> RemovedList { get; } = [];



    #region Observable overrides

    protected override void InsertItem( int index, TMember item )
    {

        if( !Guard.Add(item) ) 
            return;

        item.SetParent(Owner);
        item.PropertyChanged += OnMemberPropertyChanged;

        base.InsertItem(index, item);

        Owner.NotifyCollectionChanged(this);

    }

    protected override void SetItem( int index, TMember item )
    {

        if( !Guard.Add(item) )
            return;

        item.SetParent(Owner);
        item.PropertyChanged += OnMemberPropertyChanged;

        base.SetItem(index, item);

        Owner.NotifyCollectionChanged(this);

    }

    protected override void ClearItems()
    {

        foreach (var item in this)
        {

            if( !Guard.Remove(item) ) 
                continue;

            item.SetParent(null);
            item.PropertyChanged -= OnMemberPropertyChanged;

            if ( item.IsAdded() )
                continue;

            item.Removed();
            RemovedList.Add(item);

        }

        base.ClearItems();

        Owner.NotifyCollectionChanged(this);


    }

    protected override void RemoveItem( int index )
    {

        var item = this[index];
        if( !Guard.Remove(item) )
            return;


        item.SetParent(null);
        item.PropertyChanged -= OnMemberPropertyChanged;


        if( !item.IsAdded() )
        {
            item.Removed();

            RemovedList.Add(item);
        }

        base.RemoveItem(index);

        Owner.NotifyCollectionChanged(this);

    }

    #endregion


    #region IDependentCollection implementation


    public bool IsModified()
    {
        var modified = Guard.Any(m => m.IsModified() || m.IsAdded()) || RemovedList.Count > 0;
        return modified;
    }

    void IDependentCollection.AddMember( IModel item )
    {

        if (item is TMember member)
            Add( member );
    }

    void IDependentCollection.RemoveMember( IModel item )
    {
        if (item is TMember member)
            Remove(member);
    }

    IEnumerable<IModel> IDependentCollection.Members => Guard;

    IEnumerable<IModel> IDependentCollection.DeltaMembers => Guard.Where(m => m.IsModified() || m.IsAdded()).Union(RemovedList);

    #endregion


    #region IEditableObject implementation

    public void BeginEdit()
    {
        foreach (var e in Guard)
            e.BeginEdit();
    }

    public void CancelEdit()
    {

        Undo();
    }


    public void EndEdit()
    {
        foreach (var e in Guard)
            e.EndEdit();
    }


    public void Undo()
    {

        var added = new List<TMember>();

        foreach (var mem in Guard)
        {
            if (mem.IsAdded())
                added.Add(mem);
            else
                mem.Undo();
        }

        foreach (var mem in added)
        {
            mem.PropertyChanged -= OnMemberPropertyChanged;
            Remove(mem);
        }

        foreach( var mem in RemovedList )
        {

            mem.Undo();

            mem.PropertyChanged += OnMemberPropertyChanged;
            Add( mem );

        }

        RemovedList.Clear();


    }

    public void Post()
    {

        RemovedList.Clear();

        foreach (var mem in Guard)
            mem.Post();

    }

    #endregion


}