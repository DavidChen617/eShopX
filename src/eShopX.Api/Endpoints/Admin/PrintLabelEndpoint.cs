using Infrastructure.Logistics;
using Infrastructure.Logistics.EcPay;

namespace eShopX.Endpoints.Admin;

public sealed class PrintLabelEndpoint : IGroupedEndpoint<AdminOrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/print-label", Handle);
    }

    private static async Task<IResult> Handle(
        PrintLabelRequest request,
        EcPayPrintTradeDocumentClient ecpay,
        CancellationToken ct)
    {
        var html = await ecpay.PrintTradeDocumentAsync(
            request.LogisticsId,
            request.LogisticsSubType,
            ct);

        return Results.Content(html, "text/html");
    }
}

public record PrintLabelRequest(string LogisticsId, string LogisticsSubType);
