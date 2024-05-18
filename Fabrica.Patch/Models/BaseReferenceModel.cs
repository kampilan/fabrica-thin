namespace Fabrica.Patch.Models;

public abstract class BaseReferenceModel: IReferenceModel
{

    public abstract string GetUid();


    private Type GetUnproxiedType()
    {
        return GetType();
    }


    public virtual bool Equals( BaseReferenceModel? other )
    {

        if (other == null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Equals(GetUid(), other.GetUid()))
        {

            var typeOther = other.GetUnproxiedType();
            var typeThis  = GetUnproxiedType();

            return (typeThis.IsAssignableFrom(typeOther)) || (typeOther.IsAssignableFrom(typeThis));

        }

        return false;

    }



    public override bool Equals(object? other)
    {
        if (other is BaseReferenceModel a)
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
        var s = $"{GetType().FullName} - Id: {GetUid()}";
        return s;
    }


}