using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Mapper;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using CoreMesh.Validation.Abstractions;
using CoreMesh.Validation.Abstractions.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Products;
using eShopX.Domain.ValueObjects;

namespace eShopX.Application.UseCases.Products;

public record SkuRequest(Guid? SizeId, decimal Price, int Stock);
public record ImageRequest(string Url, string PublicId, bool IsPrimary, int SortOrder);

public record AddProductVariantCommand(
    Guid ProductId,
    string Color,
    IReadOnlyList<SkuRequest> Skus) : IRequest<Result<AddProductVariantResponse>>, IValidatable<AddProductVariantCommand>
{
    public void ConfigureValidateRules(IValidationBuilder<AddProductVariantCommand> builder)
    {
        builder.For(x => x.Color)
            .NotEmpty("Color is required.");

        builder.For(x => x.Skus)
            .NotEmpty("At least one SKU is required.");
    }
}

public record AddProductVariantResponse(Guid VariantId, string Color)
    : IMapFrom<ProductVariant, AddProductVariantResponse>
{
    public AddProductVariantResponse() : this(default, null!) { }

    public AddProductVariantResponse MapFrom(ProductVariant source) =>
        new(source.Id, source.Color);
}

public class AddProductVariantHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IValidator validator,
    IMapper mapper) : IRequestHandler<AddProductVariantCommand, Result<AddProductVariantResponse>>
{
    public async Task<Result<AddProductVariantResponse>> Handle(
        AddProductVariantCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(command);
        if (!validation.IsValid)
            return Result<AddProductVariantResponse>.Invalid(validation.Errors);

        var product = await productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Result<AddProductVariantResponse>.NotFound(new Error("product_not_found", "Product not found."));

        var variant = product.AddVariant(command.Color);

        foreach (var sku in command.Skus)
            variant.AddSku(sku.SizeId, Money.Of(sku.Price), sku.Stock);

        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AddProductVariantResponse>.Ok(mapper.Map<ProductVariant, AddProductVariantResponse>(variant));
    }
}
