using ApplicationCore.Entities;
using ApplicationCore.UseCases.Orders;
using ApplicationCore.UseCases.Orders.CancelOrder;

using eShopX.Common.Exceptions;

using UnitTests.Helpers;

namespace UnitTests.UseCases.Orders;

public class CancelOrderHandlerTests
{
    [Fact]
    public async Task Handle_CompletedOrder_ThrowsConflictException()
    {
        var order = new Order { Id = Guid.NewGuid(), Status = OrderStatus.Completed };
        var orderRepo = new InMemoryRepository<Order>([order]);
        var productRepo = new InMemoryRepository<Product>();
        var orderItemRepo = new InMemoryRepository<OrderItem>();

        var handler = new CancelOrderHandler(orderRepo, productRepo, orderItemRepo);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CancelledOrder_ThrowsConflictException()
    {
        var order = new Order { Id = Guid.NewGuid(), Status = OrderStatus.Cancelled };
        var orderRepo = new InMemoryRepository<Order>([order]);
        var productRepo = new InMemoryRepository<Product>();
        var orderItemRepo = new InMemoryRepository<OrderItem>();

        var handler = new CancelOrderHandler(orderRepo, productRepo, orderItemRepo);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PaidOrder_CancelsAndRestocks()
    {
        var order = new Order { Id = Guid.NewGuid(), Status = OrderStatus.Paid };
        var product = new Product { Id = Guid.NewGuid(), StockQuantity = 1 };
        var item = new OrderItem { Id = Guid.NewGuid(), OrderId = order.Id, ProductId = product.Id, Quantity = 2 };

        var orderRepo = new InMemoryRepository<Order>([order]);
        var productRepo = new InMemoryRepository<Product>([product]);
        var orderItemRepo = new InMemoryRepository<OrderItem>([item]);

        var handler = new CancelOrderHandler(orderRepo, productRepo, orderItemRepo);

        var response = await handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        Assert.Equal(OrderStatus.Cancelled, response.Status);
        Assert.Equal(3, product.StockQuantity);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }
}
