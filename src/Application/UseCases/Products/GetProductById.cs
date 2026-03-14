using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Mapper;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Products;

namespace eShopX.Application.UseCases.Products;

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
    IMapper mapper) : IRequestHandler<GetProductByIdQuery, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(
        GetProductByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(query.ProductId, cancellationToken);
        if (product is null)
            return Result<ProductResponse>.NotFound(new Error("product_not_found", "Product not found."));

        return Result<ProductResponse>.Ok(mapper.Map<Product, ProductResponse>(product));
    }
}
