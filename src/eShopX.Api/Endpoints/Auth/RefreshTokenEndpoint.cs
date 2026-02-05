using ApplicationCore.UseCases.Auth.RefreshToken;

namespace eShopX.Endpoints.Auth;

public class RefreshTokenEndpoint : IGroupedEndpoint<AuthGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/refresh", Handle)
            .Accepts<RefreshTokenCommand>(MediaTypeNames.Application.Json)
            .Produces<RefreshTokenResponse>()
            .WithDescription("刷新 JwtToken");
    }

    private static async Task<IResult> Handle([FromBody] RefreshTokenCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return Results.Ok(ApiResponse<RefreshTokenResponse>.Success(result, "Token 更新成功"));
    }
}