namespace ApplicationCore.UseCases.Orders.GetOrder;

public class GetOrderMappingProfile : Profile
{
    public GetOrderMappingProfile()
    {
        CreateMap<OrderItem, QueryOrderItem>()
            .ConstructUsing(src => new QueryOrderItem(
                src.Id,
                src.ProductId,
                src.ProductName,
                src.UnitPrice,
                src.Quantity,
                src.UnitPrice * src.Quantity));

        CreateMap<Order, GetOrderResponse>()
            .ConstructUsing((src, ctx) => new GetOrderResponse(
                src.Id,
                src.UserId,
                src.Status,
                src.TotalAmount,
                src.PaymentMethod,
                src.PaidAt,
                src.ShippingName,
                src.ShippingAddress,
                src.ShippingPhone,
                src.CreatedAt,
                src.Items.Select(ctx.Mapper.Map<QueryOrderItem>).ToList()));
    }
}
