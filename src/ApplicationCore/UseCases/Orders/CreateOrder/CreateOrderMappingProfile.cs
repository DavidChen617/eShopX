namespace ApplicationCore.UseCases.Orders.CreateOrder;

public class CreateOrderMappingProfile : Profile
{
    public CreateOrderMappingProfile()
    {
        CreateMap<Order, CreateOrderResponse>()
            .ConstructUsing(src => new CreateOrderResponse(
                src.Id,
                src.TotalAmount,
                src.Status,
                src.CreatedAt,
                src.PaymentMethod,
                src.PaidAt));
    }
}