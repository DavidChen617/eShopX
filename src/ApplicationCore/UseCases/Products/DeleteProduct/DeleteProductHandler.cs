using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Products.DeleteProduct;

public class DeleteProductHandler(
    IRepository<Product> productRepository,
    ICacheService cacheService)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(command.ProductId, cancellationToken);

        if (product is null)
            throw new NotFoundException("Product", command.ProductId);

        if (product.SellerId != command.SellerId)
            throw new ForbiddenException("You do not have permission to delete this product");

        productRepository.Remove(product);
        await productRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("products:list", cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:recommend", cancellationToken);
    }
}
