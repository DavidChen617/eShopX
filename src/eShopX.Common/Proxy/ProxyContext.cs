using System.Reflection;

namespace eShopX.Common.Proxy;

public sealed class ProxyContext(MethodInfo method, object?[] args)
{
    public MethodInfo Method { get; } = method;
    public object?[] Args { get; } = args;
    public object? Result { get; internal set; }
    public Exception? Exception { get; internal set; }
}