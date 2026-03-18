using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Application.UseCases.Outbox;
using Domain.Aggregates.Shipments;

namespace Application.UseCases.Orders;

public record MarkOrderAsShippedCommand(Guid OrderId, string RealLogisticsId) : IRequest<Result>;

public class MarkOrderAsShippedHandler(
    IOrderRepository orderRepository,
    IShipmentRepository shipmentRepository,
    IUserRepository userRepository,
    IOutboxEventRepository outboxEventRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkOrderAsShippedCommand, Result>
{
    public async Task<Result> Handle(
        MarkOrderAsShippedCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result.NotFound(new Error("order_not_found", "Order not found."));

        var shipment = await shipmentRepository.GetByOrderIdAsync(command.OrderId, cancellationToken);
        if (shipment is null)
            return Result.NotFound(new Error("shipment_not_found", "Shipment not found."));

        var user = await userRepository.GetByIdAsync(order.UserId, cancellationToken);
        if (user is null)
            return Result.NotFound(new Error("user_not_found", "User not found."));

        order.MarkAsShipped();
        shipment.AssignRealLogisticsId(command.RealLogisticsId);
        shipment.UpdateStatus("300", "出貨中", DateTime.UtcNow);

        var payload = BuildPayload(command.OrderId, user.Email, user.Name, command.RealLogisticsId, shipment);
        var outboxEvent = OutboxEventFactory.CreateOrderShipped(payload);

        orderRepository.Update(order);
        shipmentRepository.Update(shipment);
        await outboxEventRepository.AddAsync(outboxEvent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }

    private static OrderShippedOutboxPayload BuildPayload(
        Guid orderId, string email, string name, string logisticsId, Shipment shipment)
    {
        return shipment switch
        {
            CVSShipment cvs => new OrderShippedOutboxPayload(
                orderId, email, name, logisticsId,
                cvs.LogisticsSubType.ToString(),
                StoreName: cvs.StoreName,
                StoreId: cvs.StoreId,
                Address: null,
                ZipCode: null),

            HomeShipment home => new OrderShippedOutboxPayload(
                orderId, email, name, logisticsId,
                home.LogisticsSubType.ToString(),
                StoreName: null,
                StoreId: null,
                Address: home.Address,
                ZipCode: home.ZipCode),

            _ => new OrderShippedOutboxPayload(
                orderId, email, name, logisticsId,
                shipment.LogisticsSubType.ToString(),
                null, null, null, null)
        };
    }
}
