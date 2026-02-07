using System.Security.Claims;

using ApplicationCore.UseCases.Sellers.ApplyForSeller;

namespace eShopX.Endpoints.Sellers;

public class ApplyForSellerEndpoint : IGroupedEndpoint<SellersGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/apply", Handle)
            .Produces<ApiResponse<ApplyForSellerResponse>>()
            .WithDescription("申請成為賣家");
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

        var result = await sender.Send(new ApplyForSellerCommand(userId));
        return Results.Ok(ApiResponse<ApplyForSellerResponse>.Success(result));
    }
}
