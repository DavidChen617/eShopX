using eShopX.Application.Interfaces;
using eShopX.Application.UseCases.Logistics;

namespace eShopX.Endpoints.Orders;

public sealed class GetLogisticsStatusEndpoint : IGroupedEndpoint<OrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/logistics/status", Handle)
            .Produces(200)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        ICacher cacher,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var logistics = await cacher.GetAsync<LogisticsCacheData>(
            LogisticsCacheKeys.UserLogistics(userId), ct);

        if (logistics is null)
            return Results.Ok(new { isReady = false });

        return Results.Ok(new
        {
            isReady = true,
            logistics.LogisticsSubType,
            logistics.ReceiverStoreName,
            logistics.ReceiverAddress
        });
    }
}
