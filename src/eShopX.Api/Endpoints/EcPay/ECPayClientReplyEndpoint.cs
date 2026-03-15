using eShopX.Application.Interfaces;
using eShopX.Application.UseCases.Logistics;
using Infrastructure.Logistics.EcPay;
using Infrastructure.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.ECPay;

public sealed class EcPayClientReplyEndpoint : IGroupedEndpoint<EcPayGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/client-reply", Handle)
            .DisableAntiforgery();
    }

    private static async Task<IResult> Handle(
        [FromQuery] string token,
        [FromForm] string ResultData,
        EcPayLogisticsSelectionClient ecpay,
        ICacher cacher,
        IOptions<SiteOptions> siteOptions,
        CancellationToken ct)
    {
        var userId = await cacher.GetAsync<Guid>(LogisticsCacheKeys.LogisticsSession(token), ct);
        if (userId == Guid.Empty)
            return Results.Redirect(siteOptions.Value.FrontendDomain + "/checkout?logistics=error");

        var data = ecpay.DecryptClientReply(ResultData);

        await cacher.SetAsync(
            LogisticsCacheKeys.UserLogistics(userId),
            data,
            TimeSpan.FromMinutes(15),
            ct);

        await cacher.RemoveAsync(LogisticsCacheKeys.LogisticsSession(token), ct);

        return Results.Redirect(siteOptions.Value.FrontendDomain + "/checkout?logistics=done");
    }
}
