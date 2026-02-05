namespace ApplicationCore.UseCases.Products.GetProducts;

public record GetProductResponse(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    List<GetProductItems> Items);

public record GetProductItems(
    Guid ProductId,
    Guid? CategoryId,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    string? PrimaryImageUrl);