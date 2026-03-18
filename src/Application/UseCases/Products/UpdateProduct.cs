using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using CoreMesh.Validation.Abstractions;
using CoreMesh.Validation.Abstractions.Extensions;
using Domain.Aggregates.Products;
using Domain.ValueObjects;

namespace Application.UseCases.Products;

public record UpdateSkuRequest(Guid? SkuId, Guid? SizeId, decimal Price, int Stock);

public record UpdateVariantRequest(
    Guid? VariantId,
    string Color,
    IReadOnlyList<UpdateSkuRequest> Skus,
    IReadOnlyList<ImageRequest>? Images);

public record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string? Description,
    Audience? Audience,
    Guid CategoryId,
    IReadOnlyList<Guid>? TagIds,
    IReadOnlyList<UpdateVariantRequest>? Variants) : IRequest<Result>, IValidatable<UpdateProductCommand>
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
    IOutboxEventRepository outboxEventRepository,
    IImageStorage imageStorage,
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
        product.SyncTags(command.TagIds ?? []);

        await SyncVariantsAsync(product, command.Variants ?? [], cancellationToken);

        productRepository.Update(product);
        await outboxEventRepository.AddAsync(OutboxEventFactory.CreateProductUpsert(command.ProductId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacher.RemoveAsync(ProductCacheKeys.Product(command.ProductId), cancellationToken);

        return Result.NoContent();
    }

    private async Task SyncVariantsAsync(
        Product product,
        IReadOnlyList<UpdateVariantRequest> incomingVariants,
        CancellationToken ct)
    {
        var incomingVariantIds = incomingVariants
            .Where(v => v.VariantId.HasValue)
            .Select(v => v.VariantId!.Value)
            .ToHashSet();

        foreach (var variant in product.Variants.Where(v => !incomingVariantIds.Contains(v.Id)).ToList())
        {
            foreach (var img in variant.Images)
                await imageStorage.DeleteAsync(img.PublicId, ct);
            product.RemoveVariant(variant.Id);
        }

        foreach (var vReq in incomingVariants)
        {
            ProductVariant variant;
            if (vReq.VariantId.HasValue)
            {
                variant = product.Variants.First(v => v.Id == vReq.VariantId.Value);
                variant.UpdateColor(vReq.Color);
            }
            else
            {
                variant = product.AddVariant(vReq.Color);
            }

            SyncSkus(variant, vReq.Skus);
            await SyncImagesAsync(variant, vReq.Images ?? [], ct);
        }
    }

    private static void SyncSkus(ProductVariant variant, IReadOnlyList<UpdateSkuRequest> incomingSkus)
    {
        var incomingSkuIds = incomingSkus
            .Where(s => s.SkuId.HasValue)
            .Select(s => s.SkuId!.Value)
            .ToHashSet();

        foreach (var sku in variant.Skus.Where(s => !incomingSkuIds.Contains(s.Id)).ToList())
            variant.RemoveSku(sku.Id);

        foreach (var skuReq in incomingSkus)
        {
            if (skuReq.SkuId.HasValue)
                variant.Skus.First(s => s.Id == skuReq.SkuId.Value).Update(Money.Of(skuReq.Price), skuReq.Stock);
            else
                variant.AddSku(skuReq.SizeId, Money.Of(skuReq.Price), skuReq.Stock);
        }
    }

    private async Task SyncImagesAsync(
        ProductVariant variant,
        IReadOnlyList<ImageRequest> incomingImages,
        CancellationToken ct)
    {
        var incomingPublicIds = incomingImages.Select(i => i.PublicId).ToHashSet();

        foreach (var img in variant.Images.Where(i => !incomingPublicIds.Contains(i.PublicId)).ToList())
        {
            await imageStorage.DeleteAsync(img.PublicId, ct);
            variant.RemoveImage(img.Id);
        }

        foreach (var imgReq in incomingImages)
        {
            if (!variant.Images.Any(i => i.PublicId == imgReq.PublicId))
                variant.AddImage(imgReq.Url, imgReq.PublicId, imgReq.IsPrimary, imgReq.SortOrder);
        }
    }
}
