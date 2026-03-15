using Infrastructure.Auth.ThirdPartyAuth;
using Infrastructure.Auth.ThirdPartyAuth.Line.Models;

namespace eShopX.Endpoints.Auth;

public sealed class LineLoginEndpoint : IGroupedEndpoint<AuthGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/line", Handle)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        LineAuthRequest request,
        IThirdPartyAuthService<LineAuthRequest, LineAuthResponse> lineAuth,
        CancellationToken ct)
    {
        var response = await lineAuth.AuthAsync(request);
        return Results.Ok(response);
    }
}
