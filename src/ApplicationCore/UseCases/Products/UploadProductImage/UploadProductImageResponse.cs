namespace ApplicationCore.UseCases.Products.UploadProductImage;

public record UploadProductImageResponse(ProductImageDto Image);

public record ProductImageDto(
    Guid Id,
    Guid ProductId,
    string Url,
    string PublicId,
    string Format,
    int Width,
    int Height,
    long Bytes,
    bool IsPrimary,
    int SortOrder,
    DateTime CreatedAt);
