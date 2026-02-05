namespace ApplicationCore.UseCases.Orders.GetOrder;

public class GetOrderHandler(
    IRepository<Order> orderRepository,
    IRepository<OrderItem> orderItemRepository,
    IMapper mapper) : IRequestHandler<GetOrderQuery, GetOrderResponse>
{
    public async Task<GetOrderResponse> Handle(GetOrderQuery query, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(query.OrderId, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order", query.OrderId);
        }

        var items = await orderItemRepository.ListAsync(i => i.OrderId == query.OrderId, cancellationToken);

        return new GetOrderResponse(
            order.Id,
            order.UserId,
            order.Status,
            order.TotalAmount,
            order.PaymentMethod,
            order.PaidAt,
            order.ShippingName,
            order.ShippingAddress,
            order.ShippingPhone,
            order.CreatedAt,
            items.Select(mapper.Map<OrderItem, QueryOrderItem>).ToList()
        );
    }
}