namespace ApplicationCore.UseCases.Orders.GetUserOrders;

public class GetUserOrdersHandler(
    IRepository<Order> orderRepository,
    IRepository<User> userRepository) : IRequestHandler<GetUserOrdersQuery, GetUserOrderResponse>
{
    public async Task<GetUserOrderResponse> Handle(GetUserOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var (userid, page, pageSize) = query;
        var user = await userRepository.GetByIdAsync(userid, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", userid);
        }

        var totalCount = await orderRepository.CountAsync(o => o.UserId == user.Id, cancellationToken);

        if (totalCount == 0)
        {
            return new GetUserOrderResponse(page, pageSize, 0, 0, []);
        }

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var orders = await orderRepository.QueryAsync(q =>
            q.Where(order => order.UserId == userid)
                .OrderByDescending(order => order.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o =>
                    new QueryUserOrderItem(
                        o.Id,
                        o.Status,
                        o.TotalAmount,
                        o.Items.Count,
                        o.CreatedAt,
                        o.PaymentMethod,
                        o.PaidAt)
                ), cancellationToken);

        return new GetUserOrderResponse(page, pageSize, totalCount, totalPages, orders);
    }
}
