namespace ApplicationCore.UseCases.Products.UploadProductImage;

public record UploadProductImageCommand(
    Guid SellerId,
    Guid ProductId,
    ImageUploadRequest Image,
    bool IsPrimary,
    int SortOrder) : IRequest<UploadProductImageResponse>;