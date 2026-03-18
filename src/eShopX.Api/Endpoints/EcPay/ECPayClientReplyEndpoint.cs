using Infrastructure.Logistics.EcPay;
using Microsoft.AspNetCore.Mvc;

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
        CancellationToken ct)
    {
        var userId = await cacher.GetAsync<Guid>(LogisticsCacheKeys.LogisticsSession(token), ct);
        if (userId == Guid.Empty)
        {
            const string errorHtml = """
                <!DOCTYPE html><html><body>
                <script>
                  if (window.opener) {
                    window.opener.postMessage({ type: 'ecpay-logistics-error', message: 'Session expired' }, '*');
                  }
                  window.close();
                </script>
                </body></html>
                """;
            return Results.Content(errorHtml, "text/html");
        }

        var data = ecpay.DecryptClientReply(ResultData);

        await cacher.SetAsync(
            LogisticsCacheKeys.UserLogistics(userId),
            data,
            TimeSpan.FromMinutes(15),
            ct);

        await cacher.RemoveAsync(LogisticsCacheKeys.LogisticsSession(token), ct);

        var html = """
            <!DOCTYPE html><html><body>
            <script>
              if (window.opener) {
                window.opener.postMessage({ type: 'ecpay-logistics-done' }, '*');
              }
              window.close();
            </script>
            </body></html>
            """;
        return Results.Content(html, "text/html");
    }
}
