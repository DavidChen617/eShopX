using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Payments;

public class LinePayCancelCallbackEndpoint
    : IGroupedEndpoint<LinePayCallbackGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/cancel", Handle)
            .AllowAnonymous();
    }

    private static Task<IResult> Handle(IOptions<LinePayOptions> options)
    {
        var frontBase = string.IsNullOrWhiteSpace(options.Value.FrontendBaseUrl)
            ? "http://localhost:4200"
            : options.Value.FrontendBaseUrl.TrimEnd('/');

        var redirectUrl = $"{frontBase}/pay/line/cancel";
        return Task.FromResult(Results.Redirect(redirectUrl));
    }
}
