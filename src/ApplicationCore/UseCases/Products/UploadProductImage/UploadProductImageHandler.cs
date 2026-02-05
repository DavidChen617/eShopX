using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Products.UploadProductImage;

public class UploadProductImageHandler(
    IRepository<Product> productRepository,
    IRepository<ProductImage> productImageRepository,
    IImageStorage imageStorage,
    ICacheService cacheService)
    : IRequestHandler<UploadProductImageCommand, UploadProductImageResponse>
{
    public async Task<UploadProductImageResponse> Handle(
        UploadProductImageCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(command.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), command.ProductId);

        if (product.SellerId != command.SellerId)
        {
            throw new ForbiddenException("You do not have permission to upload image for this product");
        }

        var uploadResult = await imageStorage.UploadAsync(command.Image, cancellationToken);

        if (command.IsPrimary)
        {
            var currentPrimary = await productImageRepository.ListAsync(
                x => x.ProductId == product.Id && x.IsPrimary,
                cancellationToken);

            foreach (var image in currentPrimary)
            {
                image.IsPrimary = false;
            }

            productImageRepository.UpdateRange(currentPrimary);
        }

        var entity = new ProductImage
        {
            ProductId = product.Id,
            Url = uploadResult.Url,
            PublicId = uploadResult.PublicId,
            Format = uploadResult.Format,
            Width = uploadResult.Width,
            Height = uploadResult.Height,
            Bytes = uploadResult.Bytes,
            IsPrimary = command.IsPrimary,
            SortOrder = command.SortOrder
        };

        await productImageRepository.AddAsync(entity, cancellationToken);
        await productImageRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("products:list", cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:recommend", cancellationToken);

        var dto = new ProductImageDto(
            entity.Id,
            entity.ProductId,
            entity.Url,
            entity.PublicId,
            entity.Format,
            entity.Width,
            entity.Height,
            entity.Bytes,
            entity.IsPrimary,
            entity.SortOrder,
            entity.CreatedAt);

        return new UploadProductImageResponse(dto);
    }
}