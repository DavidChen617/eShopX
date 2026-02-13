namespace ApplicationCore.UseCases.Products.CreateProduct;

public class CreateProductProfile : Profile
{
    public CreateProductProfile()
    {

        CreateMap<CreateProductCommand, Product>()
            .ConstructUsing(src => new Product()
            {
                SellerId = src.SellerId,
                CategoryId = src.CategoryId,
                Name = src.Name,
                Description = src.Description,
                Price = src.Price,
                StockQuantity = src.StockQuantity,
                IsActive = src.IsActive,
                UpdatedAt = DateTime.UtcNow,
            });
        
        CreateMap<Product, CreateProductResponse>()
            .ConstructUsing(src => new CreateProductResponse(
                src.Id,
                src.CategoryId,
                src.Name,
                src.Description,
                src.Price,
                src.IsActive,
                src.StockQuantity,
                src.CreatedAt,
                src.UpdatedAt));
    }
}
