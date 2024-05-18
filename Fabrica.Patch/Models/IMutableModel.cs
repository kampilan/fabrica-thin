using System.ComponentModel;
using Fabrica.Patch.Builder;

namespace Fabrica.Patch.Models;

public interface IMutableModel: IModel, IEditableObject
{


    bool IsReadonly();
    void Readonly();


    bool IsAdded();

    void Added();

    bool IsModified();


    bool IsRemoved();

    void Removed();

    void Undo();
    void Post();


    IDictionary<string, DeltaProperty> GetDelta();


    void NotifyCollectionChanged( IDependentCollection collection, IModel? member = null );

    void OnCreate();
    void OnModification();





}