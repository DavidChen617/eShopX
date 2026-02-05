using System.Security.Claims;

using ApplicationCore.UseCases.Sellers.GetPendingSellers;

namespace eShopX.Endpoints.Sellers;

public class GetPendingSellersEndpoint : IGroupedEndpoint<SellersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/pending", Handle)
            .Produces<ApiResponse<GetPendingSellersResponse>>()
            .RequireAuthorization("Admin")
            .WithDescription("取得待審核的賣家申請清單");
    }

    private static async Task<IResult> Handle(ClaimsPrincipal user, ISender sender)
    {
        var adminIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
        {
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await sender.Send(new GetPendingSellersQuery(adminId));
        return Results.Ok(ApiResponse<GetPendingSellersResponse>.Success(result));
    }
}