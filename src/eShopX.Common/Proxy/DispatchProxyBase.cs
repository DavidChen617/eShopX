using System.Collections.Concurrent;
using System.Reflection;

namespace eShopX.Common.Proxy;

public abstract class DispatchProxyBase<T> : DispatchProxy where T : class
{
    private T? _target;
    private IServiceProvider? _services;
    private static readonly ConcurrentDictionary<Type, Func<DispatchProxyBase<T>, Task, ProxyContext, Task>> TaskInterceptors = new();
    private static readonly ConcurrentDictionary<Type, Func<DispatchProxyBase<T>, object, ProxyContext, object>> ValueTaskInterceptors = new();
    private static readonly ConcurrentDictionary<MethodInfo, MethodMeta> MethodMetadata = new();

    protected T Target => _target ?? throw new InvalidOperationException("Proxy target not set.");
    protected IServiceProvider Services => _services ?? throw new InvalidOperationException("ServiceProvider not set.");

    internal void SetParameters(T target, IServiceProvider services)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    protected virtual void OnBefore(ProxyContext context) { }
    protected virtual void OnAfter(ProxyContext context) { }
    protected virtual void OnError(ProxyContext context, Exception exception) { }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null) throw new ArgumentNullException(nameof(targetMethod));

        var callArgs = args ?? Array.Empty<object?>();
        var context = new ProxyContext(targetMethod, callArgs);
        OnBefore(context);

        object? result;
        try
        {
            result = DispatchProxyInvoker.Invoke(targetMethod, Target, callArgs);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            context.Exception = ex.InnerException;
            OnError(context, ex.InnerException);
            throw ex.InnerException;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            OnError(context, ex);
            throw;
        }

        var meta = MethodMetadata.GetOrAdd(targetMethod, BuildMethodMeta);
        return meta.Kind switch
        {
            ReturnKind.Task => InterceptTaskNonGeneric((Task)result!, context),
            ReturnKind.TaskT => InterceptTask((Task)result!, context, meta.ResultType!),
            ReturnKind.ValueTask => InterceptValueTaskNonGeneric((ValueTask)result!, context),
            ReturnKind.ValueTaskT => InterceptValueTask((object)result!, context, meta.ResultType!),
            _ => CompleteSync(result, context)
        };
    }

    private object? CompleteSync(object? result, ProxyContext context)
    {
        context.Result = result;
        OnAfter(context);
        return result;
    }

    private object InterceptTask(Task task, ProxyContext context, Type resultType)
    {
        var interceptor = TaskInterceptors.GetOrAdd(resultType, CreateTaskInterceptor);
        return interceptor(this, task, context);
    }

    private async Task InterceptTaskNonGeneric(Task task, ProxyContext context)
    {
        try
        {
            await task.ConfigureAwait(false);
            OnAfter(context);
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            OnError(context, ex);
            throw;
        }
    }

    private async Task<TResult> InterceptTaskGeneric<TResult>(Task task, ProxyContext context)
    {
        try
        {
            var typedTask = (Task<TResult>)task;
            var result = await typedTask.ConfigureAwait(false);
            context.Result = result;
            OnAfter(context);
            return result;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            OnError(context, ex);
            throw;
        }
    }

    private static Func<DispatchProxyBase<T>, Task, ProxyContext, Task> CreateTaskInterceptor(Type resultType)
    {
        var method = typeof(DispatchProxyBase<T>)
            .GetMethod(nameof(BuildTaskInterceptor), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(resultType);

        return (Func<DispatchProxyBase<T>, Task, ProxyContext, Task>)method.Invoke(null, null)!;
    }

    private static Func<DispatchProxyBase<T>, Task, ProxyContext, Task> BuildTaskInterceptor<TResult>()
    {
        return (proxy, task, context) => proxy.InterceptTaskGeneric<TResult>(task, context);
    }

    private object InterceptValueTask(object valueTask, ProxyContext context, Type resultType)
    {
        var interceptor = ValueTaskInterceptors.GetOrAdd(resultType, CreateValueTaskInterceptor);
        return interceptor(this, valueTask, context);
    }

    private async ValueTask InterceptValueTaskNonGeneric(ValueTask task, ProxyContext context)
    {
        try
        {
            await task.ConfigureAwait(false);
            OnAfter(context);
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            OnError(context, ex);
            throw;
        }
    }

    private async ValueTask<TResult> InterceptValueTaskGeneric<TResult>(ValueTask<TResult> task, ProxyContext context)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            context.Result = result;
            OnAfter(context);
            return result;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            OnError(context, ex);
            throw;
        }
    }

    private static Func<DispatchProxyBase<T>, object, ProxyContext, object> CreateValueTaskInterceptor(Type resultType)
    {
        var method = typeof(DispatchProxyBase<T>)
            .GetMethod(nameof(BuildValueTaskInterceptor), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(resultType);

        return (Func<DispatchProxyBase<T>, object, ProxyContext, object>)method.Invoke(null, null)!;
    }

    private static Func<DispatchProxyBase<T>, object, ProxyContext, object> BuildValueTaskInterceptor<TResult>()
    {
        return (proxy, task, context) => proxy.InterceptValueTaskGeneric((ValueTask<TResult>)task, context);
    }

    private static MethodMeta BuildMethodMeta(MethodInfo method)
    {
        var returnType = method.ReturnType;
        if (returnType == typeof(Task))
            return new MethodMeta(ReturnKind.Task, null);
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            return new MethodMeta(ReturnKind.TaskT, returnType.GetGenericArguments()[0]);
        if (returnType == typeof(ValueTask))
            return new MethodMeta(ReturnKind.ValueTask, null);
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
            return new MethodMeta(ReturnKind.ValueTaskT, returnType.GetGenericArguments()[0]);

        return new MethodMeta(ReturnKind.Sync, null);
    }

    private enum ReturnKind
    {
        Sync,
        Task,
        TaskT,
        ValueTask,
        ValueTaskT
    }

    private sealed record MethodMeta(ReturnKind Kind, Type? ResultType);
}