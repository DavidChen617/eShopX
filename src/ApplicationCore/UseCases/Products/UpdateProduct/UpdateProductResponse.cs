namespace ApplicationCore.UseCases.Products.UpdateProduct;

public record UpdateProductResponse(
    Guid ProductId,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
    );
