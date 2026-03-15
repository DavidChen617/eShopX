using eShopX.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace eShopX.Endpoints.Products;

public sealed class SearchProductsEndpoint : IGroupedEndpoint<ProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/search", Handle);
    }

    private static async Task<IResult> Handle(
        [FromQuery] string? keyword,
        [FromQuery] Guid? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? isActive,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        IProductSearcher searchService,
        CancellationToken ct)
    {
        var query = new ProductSearchQuery(
            keyword,
            categoryId,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            IsActive: isActive,
            Page: page <= 0 ? 1 : page,
            PageSize: pageSize <= 0 ? 20 : pageSize);

        var result = await searchService.SearchAsync(query, ct);
        return Results.Ok(result);
    }
}
