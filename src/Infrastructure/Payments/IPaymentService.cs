using eShopX.Common.Proxy;

namespace Infrastructure.Payments;

[UseDispatchProxy(typeof(LoggingProxy<>))]
public interface IPaymentService<TCreateRequest, TCreateResponse, TConfirmRequest, TConfirmResponse>
    : IInterceptable
{
    Task<TCreateResponse> CreateAsync(TCreateRequest request, CancellationToken ct = default);
    Task<TConfirmResponse> ConfirmAsync(TConfirmRequest request, CancellationToken ct = default);
}
