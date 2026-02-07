namespace ApplicationCore.UseCases.Products.GetProductDetail;

public class GetProductDetailProfile : Profile
{
    public GetProductDetailProfile()
    {
        CreateMap<Product, GetProductDetailResponse>()
            .ConstructUsing(src => new GetProductDetailResponse(
                src.Id,
                src.CategoryId,
                src.SellerId,
                null,
                null,
                null,
                src.Name,
                src.Description,
                src.Price,
                src.StockQuantity,
                src.IsActive,
                src.CreatedAt,
                null,
                []));
    }
}
