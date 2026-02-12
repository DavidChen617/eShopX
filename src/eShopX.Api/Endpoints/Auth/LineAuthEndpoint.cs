using Infrastructure.Auth.ThirdPartyAuth;

namespace eShopX.Endpoints.Auth;

public class LineAuthEndpoint : IGroupedEndpoint<AuthGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/line/callback", HandleAsync)
            .Accepts<LineAuthRequest>(MediaTypeNames.Application.Json)
            .Produces<ApiResponse<LineAuthResponse>>();
    }

    public async Task<IResult> HandleAsync(
        [FromBody] LineAuthRequest request,
        [FromServices] IThirdPartyAuthService<LineAuthRequest, LineAuthResponse> lineAuthService)
    {
        try
        {
            var result = await lineAuthService.AuthAsync(request);
            return Results.Ok(ApiResponse<LineAuthResponse>.Success(result));
        }
        catch (Exception e)
        {
            var message = e.InnerException is null ? e.Message : e.InnerException.Message;
            return Results.BadRequest(ApiResponse.Error(StatusCodes.Status400BadRequest, message));
        }
    }
}
