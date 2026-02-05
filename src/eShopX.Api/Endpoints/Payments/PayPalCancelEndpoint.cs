using Infrastructure.Options;

using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Payments;

public class PayPalCancelEndpoint(IOptions<PayPalOptions> options)
    : IGroupedEndpoint<PayPalCallbackGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/cancel", HandleAsync)
            .AllowAnonymous();
    }

    public Task<IResult> HandleAsync()
    {
        var front = string.IsNullOrWhiteSpace(options.Value.FrontendBaseUrl)
            ? "http://localhost:4200"
            : options.Value.FrontendBaseUrl.TrimEnd('/');

        return Task.FromResult(Results.Redirect($"{front}/pay/paypal/cancel"));
    }
}