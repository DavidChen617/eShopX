using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Products.DeleteProductImage;

public class DeleteProductImageHandler(
    IRepository<Product> productRepository,
    IRepository<ProductImage> productImageRepository,
    IImageStorage imageStorage,
    ICacheService cacheService)
    : IRequestHandler<DeleteProductImageCommand>
{
    public async Task Handle(DeleteProductImageCommand command, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(command.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), command.ProductId);

        if (product.SellerId != command.SellerId)
        {
            throw new ForbiddenException("You do not have permission to delete this product image");
        }

        var image = await productImageRepository.GetByIdAsync(command.ImageId, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductImage), command.ImageId);

        if (image.ProductId != command.ProductId)
        {
            throw new NotFoundException(nameof(ProductImage), command.ImageId);
        }

        await imageStorage.DeleteAsync(image.PublicId, cancellationToken);

        productImageRepository.Remove(image);
        await productImageRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("products:list", cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:recommend", cancellationToken);
    }
}
