namespace ApplicationCore.UseCases.Orders.UpdateOrderStatus;

public class UpdateOrderStatusHandler(IRepository<Order> orderRepository)
    : IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResponse>
{
    public async Task<UpdateOrderStatusResponse> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        var previousStatus = order.Status;

        ValidateStatusTransition(previousStatus, command.Status);

        order.Status = command.Status;
        await orderRepository.SaveChangesAsync(cancellationToken);

        return new UpdateOrderStatusResponse(
            order.Id,
            previousStatus,
            order.Status,
            DateTime.UtcNow);
    }

    private static void ValidateStatusTransition(OrderStatus from, OrderStatus to)
    {
        var validTransitions = new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.Pending] = [OrderStatus.Paid, OrderStatus.Cancelled],
            [OrderStatus.Paid] = [OrderStatus.Completed, OrderStatus.Cancelled],
            [OrderStatus.Shipped] = [OrderStatus.Completed],
            [OrderStatus.Completed] = [],
            [OrderStatus.Cancelled] = []
        };

        if (!validTransitions.TryGetValue(from, out var allowed) || !allowed.Contains(to))
        {
            throw new ValidationException("Status", $"Cannot transition from {from} to {to}");
        }
    }
}