using Application.UseCases.Payments;
using Domain.Aggregates.Payments;
using Infrastructure.Options;
using Infrastructure.Payments.Line;
using Infrastructure.Payments.Line.Models;
using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Payments;

public sealed class LinePayConfirmCallbackEndpoint : IGroupedEndpoint<PaymentsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/linepay/confirm", Handle)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        long transactionId,
        Guid orderId,
        IDispatcher dispatcher,
        IPaymentRepository paymentRepository,
        LinePayService linePayService,
        IOptions<SiteOptions> siteOptions,
        CancellationToken ct)
    {
        var frontendDomain = siteOptions.Value.FrontendDomain;

        var payment = await paymentRepository.GetByOrderIdAsync(orderId, ct);
        if (payment is null)
            return Results.Redirect($"{frontendDomain}/orders?payment=error");

        if (payment.Status == PaymentStatus.Paid)
            return Results.Redirect($"{frontendDomain}/orders/{orderId}?payment=success");

        var confirmResponse = await linePayService.ConfirmPaymentAsync(
            transactionId,
            new LinePayConfirmRequest(payment.Amount.Amount, "TWD"),
            ct);

        if (confirmResponse.ReturnCode != "0000")
            return Results.Redirect($"{frontendDomain}/orders/{orderId}?payment=error");

        var confirmResult = await dispatcher.Send(
            new ConfirmPaymentCommand(orderId, transactionId.ToString()), ct);
        if (!confirmResult.IsSuccess)
            return Results.Redirect($"{frontendDomain}/orders/{orderId}?payment=error");

        return Results.Redirect($"{frontendDomain}/orders/{orderId}?payment=success");
    }
}
