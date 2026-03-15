using eShopX.Api.Endpoints.Orders;
using eShopX.Application.UseCases.Logistics;
using Infrastructure.Logistics;

namespace eShopX.Endpoints.Orders;

public sealed class StartLogisticsSelectionEndpoint : IGroupedEndpoint<OrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/logistics/start", Handle);
    }

    private static async Task<IResult> Handle(
        StartLogisticsSelectionRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        IECPayLogisticsService ecpay,
        IHttpContextAccessor httpContextAccessor,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await dispatcher.Send(new StartLogisticsSelectionCommand(userId), ct);
        if (!result.IsSuccess) 
            return result.ToHttpResult();

        var token = result.Data!.Token;
        var httpContext = httpContextAccessor.HttpContext!;
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var serverReplyUrl = $"{baseUrl}/api/ecpay/client-reply?token={token}";

        var html = await ecpay.GetSelectionFormHtmlAsync(
            serverReplyUrl,
            request.GoodsAmount,
            request.ReceiverName,
            request.ReceiverPhone,
            ct);

        return Results.Content(html, "text/html");
    }
}

public record StartLogisticsSelectionRequest(
    string ReceiverName,
    string ReceiverPhone,
    decimal GoodsAmount);
