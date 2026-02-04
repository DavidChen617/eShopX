using System.Security.Claims;
using ApplicationCore.UseCases.Users.UpdateMe;

namespace eShopX.Endpoints.Users;

public class UpdateMeEndpoint : IGroupedEndpoint<UsersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/me", Handle)
            .Accepts<UpdateMeRequest>(MediaTypeNames.Application.Json)
            .WithDescription("更新使用者基本資料");
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        [FromBody] UpdateMeRequest request,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        await sender.Send(new UpdateMeCommand(userId, request.Name, request.Phone, request.Address));
        return Results.Ok(ApiResponse.NoContent("更新成功"));
    }
}

public record UpdateMeRequest(
    string Name,
    string Phone,
    string? Address);
