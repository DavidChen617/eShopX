using eShopX.Application.Interfaces;
using eShopX.Application.UseCases.Logistics;
using Infrastructure.Logistics;
using Microsoft.AspNetCore.Mvc;

namespace eShopX.Endpoints.ECPay;

public sealed class EcPayClientReplyEndpoint : IGroupedEndpoint<EcPayGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/client-reply", Handle).DisableAntiforgery();
    }

    private static async Task<IResult> Handle(
        [FromQuery] string token,
        [FromForm] string ResultData,
        IECPayLogisticsService ecpay,
        ICacher cacher,
        CancellationToken ct)
    {
        var userId = await cacher.GetAsync<Guid>(LogisticsCacheKeys.LogisticsSession(token), ct);
        if (userId == Guid.Empty)
            return Results.BadRequest("Session expired or invalid.");

        var data = ecpay.DecryptClientReply(ResultData);

        await cacher.SetAsync(
            LogisticsCacheKeys.UserLogistics(userId),
            data,
            TimeSpan.FromMinutes(15),
            ct);

        return Results.Ok();
    }
}
