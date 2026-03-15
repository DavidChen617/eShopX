namespace eShopX.Application.Interfaces;

public interface IProductSearchService
{
    Task<ProductSearchResponse> SearchAsync(ProductSearchQuery query, CancellationToken cancellationToken = default);
}

public record ProductSearchQuery(
    string? Keyword = null,
    Guid? CategoryId = null,
    Guid? SellerId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20);

public record ProductSearchResponse(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyList<ProductSearchItem> Items);

public record ProductSearchItem(
    Guid ProductId,
    Guid? CategoryId,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    string? PrimaryImageUrl);
