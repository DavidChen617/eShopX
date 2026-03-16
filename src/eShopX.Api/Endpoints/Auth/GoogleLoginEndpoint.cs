using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Infrastructure.Auth.ThirdPartyAuth;
using Infrastructure.Auth.ThirdPartyAuth.Google.Models;

namespace eShopX.Endpoints.Auth;

public sealed class GoogleLoginEndpoint : IGroupedEndpoint<AuthGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/google", Handle)
            .Produces<ApiResponse<GoogleAuthResponse>>(200)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        GoogleAuthRequest request,
        IThirdPartyAuthService<GoogleAuthRequest, GoogleAuthResponse> googleAuth,
        CancellationToken ct)
    {
        var response = await googleAuth.AuthAsync(request, ct);
        return Result<GoogleAuthResponse>.Ok(response).ToHttpResult();
    }
}
