using ApplicationCore.UseCases.Products.UploadProductImage;

namespace ApplicationCore.UseCases.Products.GetProductImages;

public class GetProductImagesHandler(
    IRepository<Product> productRepository,
    IRepository<ProductImage> productImageRepository)
    : IRequestHandler<GetProductImagesQuery, GetProductImagesResponse>
{
    public async Task<GetProductImagesResponse> Handle(
        GetProductImagesQuery query,
        CancellationToken cancellationToken = default)
    {
        var exists = await productRepository.AnyAsync(
            x => x.Id == query.ProductId,
            cancellationToken);

        if (!exists)
        {
            throw new NotFoundException(nameof(Product), query.ProductId);
        }

        var images = await productImageRepository.ListAsync(
            x => x.ProductId == query.ProductId,
            cancellationToken);

        var ordered = images
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new ProductImageDto(
                x.Id,
                x.ProductId,
                x.Url,
                x.PublicId,
                x.Format,
                x.Width,
                x.Height,
                x.Bytes,
                x.IsPrimary,
                x.SortOrder,
                x.CreatedAt))
            .ToList();

        return new GetProductImagesResponse(ordered);
    }
}
