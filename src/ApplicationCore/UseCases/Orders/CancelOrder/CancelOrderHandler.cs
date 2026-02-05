namespace ApplicationCore.UseCases.Orders.CancelOrder;

public class CancelOrderHandler(
    IRepository<Order> orderRepository,
    IRepository<Product> productRepository,
    IRepository<OrderItem> orderItemRepository) : IRequestHandler<CancelOrderCommand, CancelOrderResponse>
{
    public async Task<CancelOrderResponse> Handle(CancelOrderCommand request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order", request.OrderId);
        }

        if (order.Status == OrderStatus.Completed)
        {
            throw new ConflictException(
                $"Order {request.OrderId} cannot be cancelled because it is already Completed.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new ConflictException(
                $"Order {request.OrderId} cannot be cancelled because it is already Cancelled.");
        }

        var items =
            await orderItemRepository.ListAsync(item => item.OrderId == request.OrderId, cancellationToken);
        var products =
            await productRepository.ListAsync(product => items.Select(i => i.ProductId).Contains(product.Id),
                cancellationToken);

        foreach (var item in items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            product.StockQuantity += item.Quantity;
        }

        order.Status = OrderStatus.Cancelled;
        orderRepository.Update(order);

        productRepository.UpdateRange(products);
        await productRepository.SaveChangesAsync(cancellationToken);

        return new CancelOrderResponse(order.Id, order.Status, DateTime.UtcNow);
    }
}