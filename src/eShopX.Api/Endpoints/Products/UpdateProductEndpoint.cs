using System.Security.Claims;

using ApplicationCore.UseCases.Products.UpdateProduct;

namespace eShopX.Endpoints.Products;

public class UpdateProductEndpoint : IGroupedEndpoint<ProductsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/{productId:guid}", Handle)
            .Accepts<UpdateProductRequest>(MediaTypeNames.Application.Json)
            .WithDescription("更新商品資訊");
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        [FromRoute] Guid productId,
        [FromBody] UpdateProductRequest request,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        await sender.Send(new UpdateProductCommand(
            userId,
            productId,
            request.CategoryId,
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.IsActive));
        return Results.Ok(ApiResponse.NoContent("更新成功"));
    }
}

public record UpdateProductRequest(
    Guid? CategoryId,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive);