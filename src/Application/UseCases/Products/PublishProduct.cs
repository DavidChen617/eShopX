using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Products;

public record PublishProductCommand(Guid ProductId) : IRequest<Result>;

public class PublishProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<PublishProductCommand, Result>
{
    public async Task<Result> Handle(
        PublishProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Result.NotFound(new Error("product_not_found", "Product not found."));

        product.Publish();
        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
