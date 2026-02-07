using ApplicationCore.UseCases.Products.UploadProductImage;

namespace ApplicationCore.UseCases.Products.GetProductDetail;

public class GetProductDetailHandler(
    IRepository<Product> productRepository,
    IRepository<User> userRepository,
    IRepository<ProductImage> productImageRepository) :
    IRequestHandler<GetProductDetailQuery, GetProductDetailResponse>
{
    public async Task<GetProductDetailResponse> Handle(GetProductDetailQuery request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException("Product", request.ProductId);
        }

        var images = await productImageRepository.ListAsync(
            x => x.ProductId == product.Id,
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

        var primaryUrl = ordered.FirstOrDefault(x => x.IsPrimary)?.Url;
        User? seller = null;
        if (product.SellerId.HasValue)
        {
            seller = await userRepository.GetByIdAsync(product.SellerId.Value, cancellationToken);
        }

        return new GetProductDetailResponse(
            product.Id,
            product.CategoryId,
            seller?.Id,
            seller?.Name,
            seller?.Email,
            seller?.Phone,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.IsActive,
            product.CreatedAt,
            primaryUrl,
            ordered);
    }
}
