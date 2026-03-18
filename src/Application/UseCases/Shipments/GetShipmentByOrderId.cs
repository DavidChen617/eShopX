using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;

namespace Application.UseCases.Shipments;

public record GetShipmentByOrderIdQuery(Guid OrderId) : IRequest<Result<ShipmentResponse>>;

public record ShipmentResponse(
    Guid ShipmentId,
    Guid OrderId,
    string LogisticsId,
    string LogisticsSubType,
    string LogisticsStatus,
    string LogisticsStatusName,
    DateTime CreatedAt);

public class GetShipmentByOrderIdHandler(
    IShipmentRepository shipmentRepository) : IRequestHandler<GetShipmentByOrderIdQuery, Result<ShipmentResponse>>
{
    public async Task<Result<ShipmentResponse>> Handle(
        GetShipmentByOrderIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var shipment = await shipmentRepository.GetByOrderIdAsync(query.OrderId, cancellationToken);
        if (shipment is null)
            return Result<ShipmentResponse>.NotFound(new Error("shipment_not_found", "Shipment not found."));

        return Result<ShipmentResponse>.Ok(new ShipmentResponse(
            shipment.Id,
            shipment.OrderId,
            shipment.LogisticsId,
            shipment.LogisticsSubType.ToString(),
            shipment.LogisticsStatus,
            shipment.LogisticsStatusName,
            shipment.CreatedAt));
    }
}
