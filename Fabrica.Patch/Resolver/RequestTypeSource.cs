using System.Reflection;
using Fabrica.Utilities.Types;

namespace Fabrica.Patch.Resolver;

public class RequestTypeSource: TypeSource
{

    protected override Func<Type, bool> GetPredicate()
    {
        return t=> t is {IsClass: true, IsAbstract: false} && t.GetCustomAttribute(typeof(ResolveAttribute)) is not null;
    }

}