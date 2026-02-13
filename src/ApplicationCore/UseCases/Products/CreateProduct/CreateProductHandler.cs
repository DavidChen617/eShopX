using ApplicationCore.UseCases.Outbox;

namespace ApplicationCore.UseCases.Products.CreateProduct;

public class CreateProductHandler(
    IRepository<Product> productRepository,
    IRepository<OutboxEvent> outboxEventRepository,
    IMapper mapper,
    ICacheService cacheService
    ) : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null)
            throw new ValidationException("create product command", "command cannot be null");

        var product = mapper.Map<CreateProductCommand, Product>(command);
        var newProduct = await productRepository.AddAsync(product, cancellationToken);
        await outboxEventRepository.AddAsync(OutboxEventFactory.CreateProductUpsert(newProduct.Id), cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("products:list", cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:recommend", cancellationToken);

        return mapper.Map<Product, CreateProductResponse>(newProduct);
    }
}
