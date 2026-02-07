namespace ApplicationCore.UseCases.Products.CreateProduct;

public record CreateProductCommand(
    Guid SellerId,
    Guid? CategoryId,
    string Name,
    string? Description,
    decimal Price,
    bool IsActive,
    int StockQuantity
) : IRequest<CreateProductResponse>;
