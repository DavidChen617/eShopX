using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Mapper;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Domain.Aggregates.Products;

namespace Application.UseCases.Products;

public record GetProductsQuery(
    Guid? CategoryId = null,
    Audience? Audience = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<GetProductsResponse>>;

public record ProductSummaryResponse(
    Guid Id,
    string Name,
    string? Audience,
    bool IsActive,
    Guid CategoryId,
    DateTime UpdatedAt,
    string? PrimaryImageUrl,
    decimal? StartingPrice,
    IReadOnlyList<string> Colors) : IMapFrom<Product, ProductSummaryResponse>
{
    public ProductSummaryResponse() : this(default, null!, null, default, default, default, null, null, []) { }

    public ProductSummaryResponse MapFrom(Product source) => new(
        source.Id,
        source.Name,
        source.Audience?.ToString(),
        source.IsActive,
        source.CategoryId,
        source.UpdatedAt,
        source.Variants
            .SelectMany(v => v.Images)
            .Where(i => i.IsPrimary)
            .OrderBy(i => i.SortOrder)
            .Select(i => i.Url)
            .FirstOrDefault(),
        source.Variants
            .SelectMany(v => v.Skus)
            .Select(s => s.Price.Amount)
            .DefaultIfEmpty()
            .Min() is var min && min == 0 ? null : min,
        source.Variants
            .Select(v => v.Color)
            .Distinct()
            .ToList()
    );
}

public record GetProductsResponse(
    IReadOnlyList<ProductSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public class GetProductsHandler(
    IProductRepository productRepository,
    ICacher cacher,
    IMapper mapper) : IRequestHandler<GetProductsQuery, Result<GetProductsResponse>>
{
    public async Task<Result<GetProductsResponse>> Handle(
        GetProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = ProductCacheKeys.ProductList(
            query.CategoryId, query.Audience, query.IsActive, query.Page, query.PageSize);

        var cached = await cacher.GetAsync<GetProductsResponse>(cacheKey, cancellationToken);
        if (cached is not null)
            return Result<GetProductsResponse>.Ok(cached);

        var (items, totalCount) = await productRepository.GetPagedAsync(
            query.CategoryId,
            query.Audience,
            query.IsActive,
            query.Page,
            query.PageSize,
            cancellationToken);

        var responses = mapper.Map<Product, ProductSummaryResponse>(items).ToList();
        var response = new GetProductsResponse(responses, totalCount, query.Page, query.PageSize);
        await cacher.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        return Result<GetProductsResponse>.Ok(response);
    }
}
