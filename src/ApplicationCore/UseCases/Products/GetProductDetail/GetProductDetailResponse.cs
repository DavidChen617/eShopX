using ApplicationCore.UseCases.Products.UploadProductImage;

namespace ApplicationCore.UseCases.Products.GetProductDetail;

public record GetProductDetailResponse(
    Guid ProductId,
    Guid? CategoryId,
    Guid? SellerId,
    string? SellerName,
    string? SellerEmail,
    string? SellerPhone,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt,
    string? PrimaryImageUrl,
    List<ProductImageDto> Images);