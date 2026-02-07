namespace ApplicationCore.UseCases.Products.GetProducts;

public record GetProductsQuery(
    string? Keyword,
    bool? IsActive,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? Page,
    int? PageSize,
    Guid? SellerId,
    Guid? CategoryId) : IRequest<GetProductResponse>;
