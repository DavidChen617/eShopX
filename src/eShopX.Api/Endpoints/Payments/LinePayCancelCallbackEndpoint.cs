using Infrastructure.Options;

using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Payments;

public class LinePayCancelCallbackEndpoint(IOptions<LinePayOptions> options)
    : IGroupedEndpoint<LinePayCallbackGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/cancel", HandleAsync)
            .AllowAnonymous();
    }

    public Task<IResult> HandleAsync()
    {
        var frontBase = string.IsNullOrWhiteSpace(options.Value.FrontendBaseUrl)
            ? "http://localhost:4200"
            : options.Value.FrontendBaseUrl.TrimEnd('/');

        var redirectUrl = $"{frontBase}/pay/line/cancel";
        return Task.FromResult(Results.Redirect(redirectUrl));
    }
}