using Application.Interfaces.Repositories;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Shipments;

namespace Application.UseCases.Orders;

public record GetOrdersQuery(
    Guid? UserId = null,
    OrderStatus? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<GetOrdersResponse>>;

public record ShipmentSummaryResponse(
    string LogisticsSubType,
    string LogisticsId,
    string ReceiverName,
    string ReceiverPhone,
    string? StoreName,
    string? Address);

public record OrderSummaryResponse(
    Guid OrderId,
    Guid UserId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    ShipmentSummaryResponse? Shipment);

public record GetOrdersResponse(
    IReadOnlyList<OrderSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public class GetOrdersHandler(
    IOrderRepository orderRepository,
    IShipmentRepository shipmentRepository) : IRequestHandler<GetOrdersQuery, Result<GetOrdersResponse>>
{
    public async Task<Result<GetOrdersResponse>> Handle(
        GetOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await orderRepository.GetPagedAsync(
            query.UserId,
            query.Status,
            query.Page,
            query.PageSize,
            cancellationToken);

        var orderIds = items.Select(o => o.Id).ToList();
        var shipments = await shipmentRepository.GetByOrderIdsAsync(orderIds, cancellationToken);
        var shipmentMap = shipments.ToDictionary(s => s.OrderId);

        var responses = items.Select(order =>
        {
            shipmentMap.TryGetValue(order.Id, out var shipment);
            var shipmentSummary = shipment switch
            {
                CVSShipment cvs => new ShipmentSummaryResponse(
                    cvs.LogisticsSubType.ToString(), cvs.LogisticsId,
                    cvs.Receiver.Name, cvs.Receiver.CellPhone,
                    cvs.StoreName, null),
                HomeShipment home => new ShipmentSummaryResponse(
                    home.LogisticsSubType.ToString(), home.LogisticsId,
                    home.Receiver.Name, home.Receiver.CellPhone,
                    null, home.Address),
                _ => null
            };
            return new OrderSummaryResponse(
                order.Id, order.UserId, order.Status.ToString(),
                order.TotalAmount.Amount, order.CreatedAt, shipmentSummary);
        }).ToList();

        return Result<GetOrdersResponse>.Ok(
            new GetOrdersResponse(responses, totalCount, query.Page, query.PageSize));
    }
}
