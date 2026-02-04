using System.Security.Claims;
using ApplicationCore.UseCases.Sellers.RejectSeller;

namespace eShopX.Endpoints.Sellers;

public class RejectSellerEndpoint : IGroupedEndpoint<SellersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{userId:guid}/reject", Handle)
            .Accepts<RejectSellerRequest>(MediaTypeNames.Application.Json)
            .Produces<ApiResponse<RejectSellerResponse>>()
            .RequireAuthorization("Admin")
            .WithDescription("管理員拒絕賣家申請");
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        [FromRoute] Guid userId,
        [FromBody] RejectSellerRequest request,
        ISender sender)
    {
        var adminIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
        {
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await sender.Send(new RejectSellerCommand(userId, adminId, request.Reason));
        return Results.Ok(ApiResponse<RejectSellerResponse>.Success(result));
    }
}

public record RejectSellerRequest(string Reason);
