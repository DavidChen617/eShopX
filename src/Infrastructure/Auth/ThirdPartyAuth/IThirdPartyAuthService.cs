namespace Infrastructure.Auth.ThirdPartyAuth;

public interface IThirdPartyAuthService<TRequest, TResponse>
{
    Task<TResponse> AuthAsync(TRequest request);
}
