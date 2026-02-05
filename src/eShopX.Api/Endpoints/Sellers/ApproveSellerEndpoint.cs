using System.Security.Claims;

using ApplicationCore.UseCases.Sellers.ApproveSeller;

namespace eShopX.Endpoints.Sellers;

public class ApproveSellerEndpoint : IGroupedEndpoint<SellersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{userId:guid}/approve", Handle)
            .Produces<ApiResponse<ApproveSellerResponse>>()
            .RequireAuthorization("Admin")
            .WithDescription("管理員核准賣家申請");
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        [FromRoute] Guid userId,
        ISender sender)
    {
        var adminIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
        {
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await sender.Send(new ApproveSellerCommand(userId, adminId));
        return Results.Ok(ApiResponse<ApproveSellerResponse>.Success(result));
    }
}