using ApplicationCore.Entities;
using ApplicationCore.UseCases.Orders;
using ApplicationCore.UseCases.Orders.UpdateOrderStatus;

using eShopX.Common.Exceptions;

using UnitTests.Helpers;

namespace UnitTests.UseCases.Orders;

public class UpdateOrderStatusHandlerTests
{
    [Fact]
    public async Task Handle_InvalidTransition_ThrowsValidationException()
    {
        var order = new Order { Id = Guid.NewGuid(), Status = OrderStatus.Pending };
        var orderRepo = new InMemoryRepository<Order>([order]);
        var handler = new UpdateOrderStatusHandler(orderRepo);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new UpdateOrderStatusCommand(order.Id, OrderStatus.Completed), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidTransition_UpdatesStatus()
    {
        var order = new Order { Id = Guid.NewGuid(), Status = OrderStatus.Pending };
        var orderRepo = new InMemoryRepository<Order>([order]);
        var handler = new UpdateOrderStatusHandler(orderRepo);

        var response = await handler.Handle(new UpdateOrderStatusCommand(order.Id, OrderStatus.Paid), CancellationToken.None);

        Assert.Equal(OrderStatus.Pending, response.PreviousStatus);
        Assert.Equal(OrderStatus.Paid, response.CurrentStatus);
        Assert.Equal(OrderStatus.Paid, order.Status);
    }
}