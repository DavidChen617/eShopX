namespace ApplicationCore.UseCases.Homepage.GetRecommendProducts;

public record GetRecommendProductsResponse(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    List<RecommendProductItem> Items);

public record RecommendProductItem(
    Guid Id,
    string Name,
    string? ImageUrl,
    decimal Price,
    decimal? OriginalPrice,
    int SalesCount);