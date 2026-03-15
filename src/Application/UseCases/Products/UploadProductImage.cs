using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Products;

public record UploadProductImageCommand(
    Guid ProductId,
    Guid VariantId,
    string FileName,
    Stream Content,
    bool IsPrimary,
    int SortOrder) : IRequest<Result<UploadProductImageResponse>>;

public record UploadProductImageResponse(Guid ImageId, string Url);

public class UploadProductImageHandler(
    IProductRepository productRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadProductImageCommand, Result<UploadProductImageResponse>>
{
    public async Task<Result<UploadProductImageResponse>> Handle(
        UploadProductImageCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Result<UploadProductImageResponse>.NotFound(new Error("product_not_found", "Product not found."));

        var variant = product.Variants.FirstOrDefault(v => v.Id == command.VariantId);
        if (variant is null)
            return Result<UploadProductImageResponse>.NotFound(new Error("variant_not_found", "Variant not found."));

        var upload = await imageStorage.UploadAsync(
            new ImageUploadRequest(command.FileName, command.Content),
            cancellationToken);

        variant.AddImage(upload.Url, upload.PublicId, command.IsPrimary, command.SortOrder);
        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var image = variant.Images.Last();
        return Result<UploadProductImageResponse>.Ok(new UploadProductImageResponse(image.Id, upload.Url));
    }
}
