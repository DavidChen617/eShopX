using System.Security.Claims;
using ApplicationCore.UseCases.Products.CreateProduct;

namespace eShopX.Endpoints.Products;

public class CreateProductEndpoint : IGroupedEndpoint<ProductsGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("", Handle)
            .Accepts<CreateProductRequest>(MediaTypeNames.Application.Json)
            .Produces<CreateProductResponse>()
            .WithDescription("新增商品");
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        [FromBody] CreateProductRequest request,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Json(
                ApiResponse.Error(StatusCodes.Status401Unauthorized, "Unauthorized"),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await sender.Send(new CreateProductCommand(
            userId,
            request.CategoryId,
            request.Name,
            request.Description,
            request.Price,
            request.IsActive,
            request.StockQuantity));

        return Results.Created(
            $"/api/products/{result.ProductId}",
            ApiResponse<CreateProductResponse>.Success(result, "Product created successfully", StatusCodes.Status201Created));
    }
}

public record CreateProductRequest(
    Guid? CategoryId,
    string Name,
    string? Description,
    decimal Price,
    bool IsActive,
    int StockQuantity);
