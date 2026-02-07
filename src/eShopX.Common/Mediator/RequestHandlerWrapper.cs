using Microsoft.Extensions.DependencyInjection;

namespace eShopX.Common.Mediator;

/// <summary>
///     Base class for request handler wrappers.
/// </summary>
public abstract class RequestHandlerWrapper<TResponse>
{
    public abstract Task<TResponse> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

/// <summary>
///     Generic implementation that wraps the actual handler with strong typing.
///     Reflection only happens once when creating this wrapper - subsequent calls are direct method invocations.
/// </summary>
public class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse().ToList();

        RequestHandlerDelegate<TResponse> handlerDelegate = () => handler.Handle(typedRequest, cancellationToken);

        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.Handle(typedRequest, next, cancellationToken);
        }

        return handlerDelegate();
    }
}

/// <summary>
///     Base class for void request handler wrappers.
/// </summary>
public abstract class RequestHandlerWrapper
{
    public abstract Task Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

/// <summary>
///     Generic implementation that wraps the actual void handler with strong typing.
/// </summary>
public class RequestHandlerWrapperImpl<TRequest> : RequestHandlerWrapper
    where TRequest : IRequest
{
    public override async Task Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, Unit>>().Reverse().ToList();

        RequestHandlerDelegate<Unit> handlerDelegate = async () =>
        {
            await handler.Handle(typedRequest, cancellationToken);
            return Unit.Value;
        };

        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.Handle(typedRequest, next, cancellationToken);
        }

        await handlerDelegate();
    }
}
