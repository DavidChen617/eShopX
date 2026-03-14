using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Products;

public record UnpublishProductCommand(Guid ProductId) : IRequest<Result>;

public class UnpublishProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UnpublishProductCommand, Result>
{
    public async Task<Result> Handle(
        UnpublishProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Result.NotFound(new Error("product_not_found", "Product not found."));

        product.Unpublish();
        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
