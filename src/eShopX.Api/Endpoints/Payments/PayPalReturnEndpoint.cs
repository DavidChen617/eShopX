using eShopX.Application.Interfaces.Repositories;
using eShopX.Application.UseCases.Payments;
using eShopX.Domain.Aggregates.Payments;
using Infrastructure.Options;
using Infrastructure.Payments.PayPal;
using Infrastructure.Payments.PayPal.Models;
using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Payments;

public sealed class PayPalReturnEndpoint : IGroupedEndpoint<PaymentsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/paypal/return", Handle);
    }

    private static async Task<IResult> Handle(
        string token,
        Guid orderId,
        IDispatcher dispatcher,
        IPaymentRepository paymentRepository,
        PayPalService payPalService,
        IOptions<SiteOptions> siteOptions,
        CancellationToken ct)
    {
        var frontendDomain = siteOptions.Value.FrontendDomain;

        var payment = await paymentRepository.GetByOrderIdAsync(orderId, ct);
        if (payment is null)
            return Results.NotFound(new { code = "payment_not_found" });

        if (payment.Status == PaymentStatus.Paid)
            return Results.Redirect($"{frontendDomain}/orders/{orderId}?payment=success");

        var captureResponse = await payPalService.ConfirmAsync(new PayPalCaptureRequest(token), ct);

        var confirmResult = await dispatcher.Send(
            new ConfirmPaymentCommand(orderId, captureResponse.Id), ct);
        if (!confirmResult.IsSuccess)
            return confirmResult.ToHttpResult();

        return Results.Redirect($"{frontendDomain}/orders/{orderId}?payment=success");
    }
}
