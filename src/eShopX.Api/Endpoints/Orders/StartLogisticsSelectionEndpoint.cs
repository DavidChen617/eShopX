using eShopX.Application.UseCases.Logistics;
using Infrastructure.Logistics.EcPay;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Orders;

public sealed class StartLogisticsSelectionEndpoint : IGroupedEndpoint<OrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/logistics/start", Handle)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        StartLogisticsSelectionRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        EcPayLogisticsSelectionClient ecpay,
        IOptions<SiteOptions> siteOptions,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await dispatcher.Send(new StartLogisticsSelectionCommand(userId), ct);
        if (!result.IsSuccess)
            return result.ToHttpResult();

        var html = await ecpay.GetSelectionFormHtmlAsync(
            ServerReplyUrl(),
            request.GoodsAmount,
            request.ReceiverName,
            request.ReceiverPhone,
            ct);

        return Results.Content(html, "text/html");

        string ServerReplyUrl() =>
            siteOptions.Value.DomainUrl + "/api/ecpay/client-reply?token=" + result.Data!.Token;
    }
}

public record StartLogisticsSelectionRequest(
    string ReceiverName,
    string ReceiverPhone,
    decimal GoodsAmount);
