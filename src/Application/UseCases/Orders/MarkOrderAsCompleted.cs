using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Orders;

public record MarkOrderAsCompletedCommand(Guid OrderId) : IRequest<Result>;

public class MarkOrderAsCompletedHandler(
    IOrderRepository orderRepository,
    IShipmentRepository shipmentRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkOrderAsCompletedCommand, Result>
{
    public async Task<Result> Handle(
        MarkOrderAsCompletedCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result.NotFound(new Error("order_not_found", "Order not found."));

        var shipment = await shipmentRepository.GetByOrderIdAsync(command.OrderId, cancellationToken);
        if (shipment is null)
            return Result.NotFound(new Error("shipment_not_found", "Shipment not found."));

        shipment.MarkAsCompleted();
        order.MarkAsCompleted();

        shipmentRepository.Update(shipment);
        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
