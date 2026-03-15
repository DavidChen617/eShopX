using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using CoreMesh.Validation.Abstractions;
using CoreMesh.Validation.Abstractions.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Products;

namespace eShopX.Application.UseCases.Products;

public record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string? Description,
    Audience? Audience,
    Guid CategoryId) : IRequest<Result>, IValidatable<UpdateProductCommand>
{
    public void ConfigureValidateRules(IValidationBuilder<UpdateProductCommand> builder)
    {
        builder.For(x => x.Name)
            .NotEmpty("Product name is required.")
            .MaxLength(200, "Product name must be at most 200 characters.");
    }
}

public class UpdateProductHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICacher cacher,
    IValidator validator) : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(
        UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(command);
        if (!validation.IsValid)
            return Result.Invalid(validation.Errors);

        var product = await productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Result.NotFound(new Error("product_not_found", "Product not found."));

        product.Update(command.Name, command.Description, command.Audience, command.CategoryId);
        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacher.RemoveAsync(ProductCacheKeys.Product(command.ProductId), cancellationToken);

        return Result.NoContent();
    }
}
