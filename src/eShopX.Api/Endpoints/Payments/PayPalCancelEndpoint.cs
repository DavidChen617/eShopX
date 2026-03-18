using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Payments;

public sealed class PayPalCancelEndpoint : IGroupedEndpoint<PaymentsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/paypal/cancel", Handle)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid orderId,
        IDispatcher dispatcher,
        IPaymentRepository paymentRepository,
        IOptions<SiteOptions> siteOptions,
        CancellationToken ct)
    {
        var payment = await paymentRepository.GetByOrderIdAsync(orderId, ct);
        if (payment is not null)
        {
            var result = await dispatcher.Send(new MarkPaymentAsFailedCommand(payment.Id), ct);
            if (!result.IsSuccess) return result.ToHttpResult();
        }

        var frontendDomain = siteOptions.Value.FrontendDomain;
        return Results.Redirect($"{frontendDomain}/orders/{orderId}?payment=cancelled");
    }
}
