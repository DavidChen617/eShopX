using System.Reflection;

namespace eShopX.Common.Proxy;

public static class DispatchProxyFactory
{
    public static TInterface Create<TInterface, TProxy>(TInterface decorated, IServiceProvider services)
        where TInterface : class
        where TProxy : DispatchProxyBase<TInterface>
    {
        var proxy = DispatchProxy.Create<TInterface, TProxy>();
        ((DispatchProxyBase<TInterface>)(object)proxy).SetParameters(decorated, services);
        return proxy;
    }
}
