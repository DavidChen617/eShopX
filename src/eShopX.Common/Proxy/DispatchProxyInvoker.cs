using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace eShopX.Common.Proxy;

internal static class DispatchProxyInvoker
{
    private static readonly ConcurrentDictionary<MethodInfo, Func<object, object?[], object?>> Cache = new();

    public static object? Invoke(MethodInfo method, object target, object?[] args)
    {
        var invoker = Cache.GetOrAdd(method, BuildInvoker);
        return invoker(target, args);
    }

    public static void Precompile(Type interfaceType)
    {
        if (!interfaceType.IsInterface)
            throw new ArgumentException("interfaceType must be an interface.", nameof(interfaceType));

        foreach (var method in interfaceType.GetMethods())
        {
            Cache.GetOrAdd(method, BuildInvoker);
        }
    }

    private static Func<object, object?[], object?> BuildInvoker(MethodInfo method)
    {
        var targetParam = Expression.Parameter(typeof(object), "target");
        var argsParam = Expression.Parameter(typeof(object[]), "args");

        var parameters = method.GetParameters();
        var argumentExpressions = new Expression[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var index = Expression.Constant(i);
            var argAccess = Expression.ArrayIndex(argsParam, index);
            var argCast = Expression.Convert(argAccess, parameters[i].ParameterType);
            argumentExpressions[i] = argCast;
        }

        var instanceCast = Expression.Convert(targetParam, method.DeclaringType!);
        var call = Expression.Call(instanceCast, method, argumentExpressions);

        Expression body;
        if (method.ReturnType == typeof(void))
        {
            body = Expression.Block(call, Expression.Constant(null, typeof(object)));
        }
        else
        {
            body = Expression.Convert(call, typeof(object));
        }

        var lambda = Expression.Lambda<Func<object, object?[], object?>>(body, targetParam, argsParam);
        return lambda.Compile();
    }
}
