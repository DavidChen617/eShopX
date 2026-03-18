using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using Domain.Aggregates.Products;

namespace Application.Interfaces;

public interface IProductSearcher : IRequestHandler<ProductSearchQuery, Result<ProductSearchResponse>>;

public record ProductSearchQuery(
    string? Keyword = null,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? IsActive = null,
    Audience? Audience = null,
    int Page = 1,
    int PageSize = 20): IRequest<Result<ProductSearchResponse>>;

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
