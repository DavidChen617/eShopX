using System.Security.Claims;
using ApplicationCore.UseCases.Users.DeleteAvatar;

namespace eShopX.Endpoints.Users;

public class DeleteUserAvatarEndpoint : IGroupedEndpoint<UsersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/me/avatar", Handle)
            .DisableAntiforgery()
            .WithDescription("刪除使用者頭像");
    }

    private static async Task<IResult> Handle(ClaimsPrincipal user, ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);

        await sender.Send(new DeleteUserAvatarCommand(userId));
        return Results.Ok(ApiResponse.NoContent("刪除成功"));
    }
}
