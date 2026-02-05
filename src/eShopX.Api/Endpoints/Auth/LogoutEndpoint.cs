using ApplicationCore.UseCases.Auth.Logout;

namespace eShopX.Endpoints.Auth;

public class LogoutEndpoint : IGroupedEndpoint<AuthGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/logout", Handle)
            .Accepts<LogoutCommand>(MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization()
            .WithDescription("登出並撤銷 Refresh Token");
    }

    private static async Task<IResult> Handle([FromBody] LogoutCommand command, ISender sender)
    {
        await sender.Send(command);
        return Results.Ok(ApiResponse.NoContent("登出成功"));
    }
}