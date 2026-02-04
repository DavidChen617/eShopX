using System.Collections.Concurrent;

namespace eShopX.Common.Mediator;

public class Sender(IServiceProvider serviceProvider) : ISender
{
    // Static cache - shared across all instances, each request type only reflects once
    private static readonly ConcurrentDictionary<Type, object> _responseWrapperCache = new();
    private static readonly ConcurrentDictionary<Type, object> _voidWrapperCache = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var handler = (RequestHandlerWrapper<TResponse>)_responseWrapperCache.GetOrAdd(
            request.GetType(),
            static requestType =>
            {
                // Reflection only happens here - once per request type
                var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse));
                return Activator.CreateInstance(wrapperType)
                       ?? throw new InvalidOperationException($"Could not create wrapper for {requestType}");
            });

        // Strong-typed call - no reflection
        return handler.Handle(request, serviceProvider, cancellationToken);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        var handler = (RequestHandlerWrapper)_voidWrapperCache.GetOrAdd(
            request.GetType(),
            static requestType =>
            {
                // Reflection only happens here - once per request type
                var wrapperType = typeof(RequestHandlerWrapperImpl<>).MakeGenericType(requestType);
                return Activator.CreateInstance(wrapperType)
                       ?? throw new InvalidOperationException($"Could not create wrapper for {requestType}");
            });

        return handler.Handle(request, serviceProvider, cancellationToken);
    }
}
