﻿using System.Collections.Specialized;
using System.ComponentModel;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Patch.Models;

public interface IDependentCollection: IEditableObject, INotifyCollectionChanged
{

    string PropertyName { get; }


    IEnumerable<IModel> Members { get; }
    void AddMember(IModel member);
    void RemoveMember(IModel member);


    bool IsModified();

    void Undo();
    void Post();

    IEnumerable<IModel> DeltaMembers { get; }


}