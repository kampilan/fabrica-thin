// ReSharper disable UnusedMember.Global
namespace Fabrica.Utilities.Cache;

public class ConcurrentResource<T>( Func<Task<IRenewedResource<T>>> factory ) : AbstractConcurrentResource<T>
{

    protected override async Task<IRenewedResource<T>> Renew()
    {
        var r = await factory();
        return r;
    }



}