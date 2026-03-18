using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Mapper;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Domain.Aggregates.Products;

namespace Application.UseCases.Products;

public record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ProductResponse>>;

public record ProductSkuResponse(Guid Id, Guid? SizeId, decimal Price, int Stock);
public record ProductImageResponse(Guid Id, string Url, bool IsPrimary, int SortOrder);
public record ProductVariantResponse(
    Guid Id,
    string Color,
    IReadOnlyList<ProductSkuResponse> Skus,
    IReadOnlyList<ProductImageResponse> Images);

public record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Audience,
    bool IsActive,
    Guid CategoryId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ProductVariantResponse> Variants,
    IReadOnlyList<Guid> TagIds) : IMapFrom<Product, ProductResponse>
{
    public ProductResponse() : this(default, null!, null, null, default, default, default, default, null!, null!) { }

    public ProductResponse MapFrom(Product source) => new(
        source.Id,
        source.Name,
        source.Description,
        source.Audience?.ToString(),
        source.IsActive,
        source.CategoryId,
        source.CreatedAt,
        source.UpdatedAt,
        source.Variants.Select(v => new ProductVariantResponse(
            v.Id,
            v.Color,
            v.Skus.Select(s => new ProductSkuResponse(s.Id, s.SizeId, s.Price.Amount, s.StockQuantity)).ToList(),
            v.Images.Select(i => new ProductImageResponse(i.Id, i.Url, i.IsPrimary, i.SortOrder)).ToList()
        )).ToList(),
        source.Tags.Select(t => t.TagId).ToList()
    );
}

public class GetProductByIdHandler(
    IProductRepository productRepository,
    ICacher cacher,
    IMapper mapper) : IRequestHandler<GetProductByIdQuery, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(
        GetProductByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = ProductCacheKeys.Product(query.ProductId);

        var cached = await cacher.GetAsync<ProductResponse>(cacheKey, cancellationToken);
        if (cached is not null)
            return Result<ProductResponse>.Ok(cached);

        var product = await productRepository.GetByIdAsync(query.ProductId, cancellationToken);
        if (product is null)
            return Result<ProductResponse>.NotFound(new Error("product_not_found", "Product not found."));

        var response = mapper.Map<Product, ProductResponse>(product);
        await cacher.SetAsync(cacheKey, response, TimeSpan.FromMinutes(30), cancellationToken);

        return Result<ProductResponse>.Ok(response);
    }
}
