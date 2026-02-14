using Infrastructure.Options;

using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Payments;

public class PayPalCancelEndpoint
    : IGroupedEndpoint<PayPalCallbackGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/cancel", Handle)
            .AllowAnonymous();
    }

    private static Task<IResult> Handle(IOptions<PayPalOptions> options)
    {
        var front = string.IsNullOrWhiteSpace(options.Value.FrontendBaseUrl)
            ? "http://localhost:4200"
            : options.Value.FrontendBaseUrl.TrimEnd('/');

        return Task.FromResult(Results.Redirect($"{front}/pay/paypal/cancel"));
    }
}
