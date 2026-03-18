using CoreMesh.Result;
using Application.UseCases.Logistics;

namespace eShopX.Endpoints.Orders;

public sealed class GetLogisticsStatusEndpoint : IGroupedEndpoint<OrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/logistics/status", Handle)
            .Produces<ApiResponse<LogisticsStatusDto>>(200)
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
            return Result<LogisticsStatusDto>.Ok(new LogisticsStatusDto(false)).ToHttpResult();

        return Result<LogisticsStatusDto>.Ok(new LogisticsStatusDto(
            true,
            logistics.LogisticsSubType,
            logistics.ReceiverStoreName,
            logistics.ReceiverAddress)).ToHttpResult();
    }
}

public record LogisticsStatusDto(
    bool IsReady,
    string? LogisticsSubType = null,
    string? StoreName = null,
    string? Address = null);
