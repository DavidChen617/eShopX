using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Products;

public record DeleteProductCommand(Guid ProductId) : IRequest<Result>;

public class DeleteProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICacher cacher) : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(
        DeleteProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Result.NotFound(new Error("product_not_found", "Product not found."));

        product.Delete();
        productRepository.Delete(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacher.RemoveAsync(ProductCacheKeys.Product(command.ProductId), cancellationToken);

        return Result.NoContent();
    }
}
