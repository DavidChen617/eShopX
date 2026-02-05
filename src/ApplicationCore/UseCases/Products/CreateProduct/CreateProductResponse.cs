namespace ApplicationCore.UseCases.Products.CreateProduct;

public record CreateProductResponse(
    Guid ProductId,
    Guid? CategoryId,
    string Name,
    string? Description,
    decimal Price,
    bool IsActive,
    int StockQuantity,
    DateTime CreatedAt,
    DateTime UpdatedAt
    );