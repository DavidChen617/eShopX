namespace ApplicationCore.UseCases.Orders.GetOrder;

public class GetOrderMappingProfile : Profile
{
    public GetOrderMappingProfile()
    {
        CreateMap<OrderItem, QueryOrderItem>()
            .ConstructUsing(src => new QueryOrderItem(
                src.ProductId,
                src.ProductName,
                src.UnitPrice,
                src.Quantity,
                src.UnitPrice * src.Quantity));
    }
}
