using eShopX.Application.UseCases.Orders;
using eShopX.Application.UseCases.Shipments;
using Infrastructure.Logistics.EcPay;

namespace eShopX.Endpoints.Admin;

public sealed class ShipOrderEndpoint : IGroupedEndpoint<AdminOrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{orderId:guid}/ship", Handle)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid orderId,
        IDispatcher dispatcher,
        EcPayCreateByTempTradeClient ecpay,
        CancellationToken ct)
    {
        var shipmentResult = await dispatcher.Send(new GetShipmentByOrderIdQuery(orderId), ct);
        if (!shipmentResult.IsSuccess) return shipmentResult.ToHttpResult();

        var shipment = shipmentResult.Data!;
        var merchantTradeNo = orderId.ToString("N")[..20];

        var decryptedJson = await ecpay.CreateByTempTradeAsync(
            shipment.LogisticsId,
            merchantTradeNo,
            ct);

        using var doc = System.Text.Json.JsonDocument.Parse(decryptedJson);
        var rtnCode = doc.RootElement.GetProperty("RtnCode").GetInt32();
        if (rtnCode != 1)
        {
            var rtnMsg = doc.RootElement.GetProperty("RtnMsg").GetString();
            return Results.BadRequest(new { code = "ecpay_error", message = rtnMsg });
        }

        var realLogisticsId = doc.RootElement.GetProperty("LogisticsID").GetString()!;

        var result = await dispatcher.Send(new MarkOrderAsShippedCommand(orderId, realLogisticsId), ct);
        if (!result.IsSuccess) return result.ToHttpResult();

        return Results.Ok(new { logisticsId = realLogisticsId, logisticsSubType = shipment.LogisticsSubType });
    }
}
