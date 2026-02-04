namespace ApplicationCore.UseCases.Products.UpdateProductImage;

public record UpdateProductImageCommand(
    Guid SellerId,
    Guid ProductId,
    Guid ImageId,
    bool? IsPrimary,
    int? SortOrder) : IRequest<UpdateProductImageResponse>;
