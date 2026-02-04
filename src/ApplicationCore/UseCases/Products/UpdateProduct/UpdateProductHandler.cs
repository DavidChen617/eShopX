using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Products.UpdateProduct;

public class UpdateProductHandler(
    IRepository<Product> productRepository,
    ICacheService cacheService) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(command.ProductId, cancellationToken);

        if (product is null)
            throw new NotFoundException("CreateProduct", command.ProductId);

        if (product.SellerId != command.SellerId)
            throw new ForbiddenException("You do not have permission to update this product");

        product.CategoryId = command.CategoryId;
        product.Price = command.Price;
        product.StockQuantity = command.StockQuantity;
        product.IsActive = command.IsActive;
        product.Name = command.Name;
        product.Description = string.IsNullOrEmpty(command.Description) ? string.Empty : command.Description;
        product.UpdatedAt = DateTime.UtcNow;

        productRepository.Update(product);
        await productRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("products:list", cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:recommend", cancellationToken);
    }
}
