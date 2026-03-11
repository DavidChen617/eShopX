namespace ApplicationCore.UseCases.Orders.GetOrder;

public class GetOrderHandler(
    IRepository<Order> orderRepository,
    IMapper mapper) : IRequestHandler<GetOrderQuery, GetOrderResponse>
{
    public async Task<GetOrderResponse> Handle(GetOrderQuery query, CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.QueryAsync(
            q => q.Include(o => o.Items).Where(o => o.Id == query.OrderId),
            cancellationToken);

        if (orders.Count == 0)
        {
            throw new NotFoundException("Order", query.OrderId);
        }

        return mapper.Map<GetOrderResponse>(orders[0]);
    }
}
