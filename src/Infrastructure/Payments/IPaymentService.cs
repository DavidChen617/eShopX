using eShopX.Common.Proxy;

namespace Infrastructure.Payments;

[UseDispatchProxy(typeof(LoggingProxy<>))]
public interface IPaymentService<TCreateRequest, TCreateResponse, TConfirmRequest, TConfirmResponse>
    : IInterceptable
{
    Task<TCreateResponse> CreateAsync(TCreateRequest request, CancellationToken ct = default);
    Task<TConfirmResponse> ConfirmAsync(TConfirmRequest request, CancellationToken ct = default);
}

[UseDispatchProxy(typeof(LoggingProxy<>))]
public interface ICreatePaymentService<TCreateRequest, TCreateResponse>: IInterceptable
{
    Task<TCreateResponse> CreateAsync(TCreateRequest request, CancellationToken ct = default);
}

[UseDispatchProxy(typeof(LoggingProxy<>))]
public interface IConfirmPaymentService<TConfirmRequest, TConfirmResponse>: IInterceptable
{
    Task<TConfirmResponse> ConfirmAsync(TConfirmRequest request, CancellationToken ct = default);
}
