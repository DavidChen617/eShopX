using System.Security.Claims;

using ApplicationCore.UseCases.Products.GetProducts;

namespace eShopX.Endpoints.Products;

public class GetMyProductsEndpoint : IGroupedEndpoint<ProductsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/mine", Handle)
            .Produces<GetProductResponse>()
            .WithDescription("取得自己的商品列表");
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        [FromQuery] string? keyword,
        [FromQuery] bool? isActive,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] Guid? categoryId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await sender.Send(new GetProductsQuery(
            keyword,
            isActive,
            minPrice,
            maxPrice,
            page,
            pageSize,
            userId,
            categoryId));

        return Results.Ok(ApiResponse<GetProductResponse>.Success(result));
    }
}
