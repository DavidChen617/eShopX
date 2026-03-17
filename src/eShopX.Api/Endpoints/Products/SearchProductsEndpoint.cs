using eShopX.Application.Interfaces;
using eShopX.Domain.Aggregates.Products;
using Microsoft.AspNetCore.Mvc;

namespace eShopX.Endpoints.Products;

public sealed class SearchProductsEndpoint : IGroupedEndpoint<ProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/search", Handle)
            .Produces<ApiResponse<ProductSearchResponse>>(200)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        [FromQuery] string? keyword,
        [FromQuery] Guid? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? isActive,
        [FromQuery] Audience? audience,
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
            Audience: audience,
            Page: page <= 0 ? 1 : page,
            PageSize: pageSize <= 0 ? 20 : pageSize);

        var result = await searchService.SearchAsync(query, ct);
        
        return result.ToHttpResult();
    }
}
