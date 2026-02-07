namespace eShopX.Common.Proxy;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
public sealed class UseDispatchProxyAttribute(Type proxyType) : Attribute
{
    public Type ProxyType { get; } = proxyType ?? throw new ArgumentNullException(nameof(proxyType));
}
