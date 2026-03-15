using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using CoreMesh.Result.Http;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Application.UseCases.Orders;
using eShopX.Application.UseCases.Payments;
using Infrastructure.Options;
using Infrastructure.Payments.PayPal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace eShopX.Api.Endpoints.Payments;

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
        IOptions<PayPalOptions> payPalOptions,
        CancellationToken ct)
    {
        var captureResponse = await payPalService.ConfirmAsync(new PayPalCaptureRequest(token), ct);

        var payment = await paymentRepository.GetByOrderIdAsync(orderId, ct);
        if (payment is null)
            return Results.NotFound(new { code = "payment_not_found" });

        var markPaidResult = await dispatcher.Send(
            new MarkPaymentAsPaidCommand(payment.Id, captureResponse.Id), ct);
        if (!markPaidResult.IsSuccess) return markPaidResult.ToHttpResult();

        var markOrderPaidResult = await dispatcher.Send(new MarkOrderAsPaidCommand(orderId), ct);
        if (!markOrderPaidResult.IsSuccess) return markOrderPaidResult.ToHttpResult();

        var frontendUrl = payPalOptions.Value.FrontendBaseUrl;
        return Results.Redirect($"{frontendUrl}/orders/{orderId}?payment=success");
    }
}
