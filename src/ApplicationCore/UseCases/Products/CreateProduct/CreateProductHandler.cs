using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Products.CreateProduct;

public class CreateProductHandler(
    IRepository<Product> productRepository,
    IMapper mapper,
    ICacheService cacheService
    ) : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null)
            throw new ValidationException("create product command", "command cannot be null");

        var product = new Product()
        {
            SellerId = command.SellerId,
            CategoryId = command.CategoryId,
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            StockQuantity = command.StockQuantity,
            IsActive = command.IsActive,
            UpdatedAt = DateTime.UtcNow,
        };

        var newProduct = await productRepository.AddAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("products:list", cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:recommend", cancellationToken);

        return mapper.Map<Product, CreateProductResponse>(newProduct);
    }
}
