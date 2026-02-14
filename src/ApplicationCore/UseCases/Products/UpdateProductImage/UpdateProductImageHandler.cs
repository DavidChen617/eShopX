using ApplicationCore.Interfaces;
using ApplicationCore.UseCases.Outbox;
using ApplicationCore.UseCases.Products.UploadProductImage;

namespace ApplicationCore.UseCases.Products.UpdateProductImage;

public class UpdateProductImageHandler(
    IRepository<Product> productRepository,
    IRepository<ProductImage> productImageRepository,
    IRepository<OutboxEvent> outboxEventRepository,
    ICacheService cacheService)
    : IRequestHandler<UpdateProductImageCommand, UpdateProductImageResponse>
{
    public async Task<UpdateProductImageResponse> Handle(
        UpdateProductImageCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(command.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), command.ProductId);

        if (product.SellerId != command.SellerId)
        {
            throw new ForbiddenException("You do not have permission to update this product image");
        }

        var image = await productImageRepository.GetByIdAsync(command.ImageId, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductImage), command.ImageId);

        if (image.ProductId != command.ProductId)
        {
            throw new NotFoundException(nameof(ProductImage), command.ImageId);
        }

        if (command.IsPrimary.HasValue)
        {
            if (command.IsPrimary.Value)
            {
                var currentPrimary = await productImageRepository.ListAsync(
                    x => x.ProductId == command.ProductId && x.IsPrimary,
                    cancellationToken);

                foreach (var item in currentPrimary)
                {
                    item.IsPrimary = false;
                }

                productImageRepository.UpdateRange(currentPrimary);
            }

            image.IsPrimary = command.IsPrimary.Value;
        }

        if (command.SortOrder.HasValue)
        {
            image.SortOrder = command.SortOrder.Value;
        }

        productImageRepository.Update(image);
        await outboxEventRepository.AddAsync(OutboxEventFactory.CreateProductUpsert(product.Id), cancellationToken);
        await productImageRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("products:list", cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:recommend", cancellationToken);

        var dto = new ProductImageDto(
            image.Id,
            image.ProductId,
            image.Url,
            image.PublicId,
            image.Format,
            image.Width,
            image.Height,
            image.Bytes,
            image.IsPrimary,
            image.SortOrder,
            image.CreatedAt);

        return new UpdateProductImageResponse(dto);
    }
}
