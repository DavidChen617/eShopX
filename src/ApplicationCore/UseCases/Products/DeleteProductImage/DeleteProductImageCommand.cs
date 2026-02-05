namespace ApplicationCore.UseCases.Products.DeleteProductImage;

public record DeleteProductImageCommand(
    Guid SellerId,
    Guid ProductId,
    Guid ImageId) : IRequest;