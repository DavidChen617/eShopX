namespace ApplicationCore.UseCases.Products.CreateProduct;

public class CreateProductProfile : Profile
{
    public CreateProductProfile()
    {
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