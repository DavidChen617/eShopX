using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Infrastructure.Logistics.EcPay;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Orders;

public sealed class StartLogisticsSelectionEndpoint : IGroupedEndpoint<OrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/logistics/start", Handle)
            .Produces<ApiResponse<string>>(200)
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

        var token = result.Data!.Token;
        var serverReplyUrl = siteOptions.Value.DomainUrl + "/api/ecpay/server-reply?token=" + token;
        var clientReplyUrl = siteOptions.Value.DomainUrl + "/api/ecpay/client-reply?token=" + token;

        var html = await ecpay.GetSelectionFormHtmlAsync(
            serverReplyUrl,
            clientReplyUrl,
            request.GoodsAmount,
            request.ReceiverName,
            request.ReceiverPhone,
            ct);
        
        return Result<string>.Ok(html).ToHttpResult();
    }
}

public record StartLogisticsSelectionRequest(
    string ReceiverName,
    string ReceiverPhone,
    decimal GoodsAmount);
