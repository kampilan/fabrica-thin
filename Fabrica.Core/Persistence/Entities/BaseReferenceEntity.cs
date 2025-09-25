namespace Fabrica.Persistence.Entities;

public abstract class BaseReferenceEntity<TImp> where TImp : BaseReferenceEntity<TImp>, IReferenceEntity
{

    public abstract string GetUid();


    private Type GetUnproxiedType()
    {
        return GetType();
    }


    public virtual bool Equals(BaseReferenceEntity<TImp>? other)
    {

        if (other == null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Equals(GetUid(), other.GetUid()))
        {

            var typeOther = other.GetUnproxiedType();
            var typeThis = GetUnproxiedType();

            return (typeThis.IsAssignableFrom(typeOther)) || (typeOther.IsAssignableFrom(typeThis));

        }

        return false;

    }



    public override bool Equals(object? other)
    {
        if (other is BaseReferenceEntity<TImp> a)
            return Equals(a);

        return false;

    }

    public override int GetHashCode()
    {
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return GetUid().GetHashCode();
    }



    public override string ToString()
    {
        var s = $"{GetType().FullName} - Uid: {GetUid()}";
        return s;
    }




}