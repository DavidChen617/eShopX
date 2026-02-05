namespace ApplicationCore.UseCases.Products.UpdateProduct;

public record UpdateProductCommand(
    Guid SellerId,
    Guid ProductId,
    Guid? CategoryId,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive) : IRequest;