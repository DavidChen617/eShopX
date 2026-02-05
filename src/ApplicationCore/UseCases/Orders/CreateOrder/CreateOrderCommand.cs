namespace ApplicationCore.UseCases.Orders.CreateOrder;

public record CreateOrderCommand(
    Guid UserId,
    string ShippingName,
    string ShippingAddress,
    string ShippingPhone,
    List<OrderItemDto> Items) : IRequest<CreateOrderResponse>;

public record OrderItemDto(Guid ProductId, int Quantity);