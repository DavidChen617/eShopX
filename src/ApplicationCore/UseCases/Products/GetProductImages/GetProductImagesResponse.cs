using ApplicationCore.UseCases.Products.UploadProductImage;

namespace ApplicationCore.UseCases.Products.GetProductImages;

public record GetProductImagesResponse(List<ProductImageDto> Images);