using Infrastructure.Auth.ThirdPartyAuth;

namespace eShopX.Endpoints.Auth;

public class GoogleAuthEndpoint : IGroupedEndpoint<AuthGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("google/callback", HandleAsync)
        .Accepts<GoogleAuthRequest>(MediaTypeNames.Application.Json)
        .Produces<ApiResponse<GoogleAuthResponse>>();
    }

    public async Task<IResult> HandleAsync(
        [FromBody] GoogleAuthRequest request,
        [FromKeyedServices("GoogleAuth")]
        IThirdPartyAuthService<GoogleAuthRequest, GoogleAuthResponse> authService)
    {
        try
        {
            return Results.Ok(ApiResponse<GoogleAuthResponse>.Success(await authService.AuthAsync(request)));
        }
        catch (Exception e)
        {
            var message = e.InnerException is null ? e.Message : e.InnerException.Message;
            return Results.BadRequest(ApiResponse.Error(StatusCodes.Status400BadRequest, message));
        }
    }
}