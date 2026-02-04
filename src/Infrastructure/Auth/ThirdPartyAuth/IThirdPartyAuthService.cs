using eShopX.Common.Proxy;

namespace Infrastructure.Auth.ThirdPartyAuth;

[UseDispatchProxy(typeof(LoggingProxy<>))]
public interface IThirdPartyAuthService<TRequest, TResponse>
    : IInterceptable
{
    Task<TResponse> AuthAsync(TRequest request);
}
