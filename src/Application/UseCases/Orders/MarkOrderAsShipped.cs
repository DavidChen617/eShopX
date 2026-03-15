using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Orders;

public record MarkOrderAsShippedCommand(Guid OrderId, string RealLogisticsId) : IRequest<Result>;

public class MarkOrderAsShippedHandler(
    IOrderRepository orderRepository,
    IShipmentRepository shipmentRepository,
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

        order.MarkAsShipped();
        shipment.AssignRealLogisticsId(command.RealLogisticsId);
        shipment.UpdateStatus("300", "出貨中", DateTime.UtcNow);

        orderRepository.Update(order);
        shipmentRepository.Update(shipment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
