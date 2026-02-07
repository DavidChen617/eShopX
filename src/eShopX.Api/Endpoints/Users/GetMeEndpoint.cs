using System.Security.Claims;

using ApplicationCore.UseCases.Users.GetMe;

namespace eShopX.Endpoints.Users;

public class GetMeEndpoint : IGroupedEndpoint<UsersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/me", Handle)
            .RequireAuthorization()
            .Produces<GetMeResponse>()
            .WithDescription("取得當前登入使用者資訊");
    }

    private static async Task<IResult> Handle(ClaimsPrincipal user, ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await sender.Send(new GetMeQuery(userId));
        return Results.Ok(ApiResponse<GetMeResponse>.Success(result));
    }
}
