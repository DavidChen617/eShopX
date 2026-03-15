using eShopX.Application.Interfaces.Repositories;
using eShopX.Application.UseCases.Payments;
using eShopX.Domain.Aggregates.Payments;
using Infrastructure.Options;
using Infrastructure.Payments.Line;
using Infrastructure.Payments.Line.Models;
using Microsoft.Extensions.Options;

namespace eShopX.Endpoints.Payments;

public sealed class LinePayConfirmCallbackEndpoint : IGroupedEndpoint<PaymentsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/linepay/confirm", Handle);
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
            return Results.NotFound(new { code = "payment_not_found" });

        if (payment.Status == PaymentStatus.Paid)
            return Results.Redirect($"{frontendDomain}/orders/{orderId}?payment=success");

        var confirmResponse = await linePayService.ConfirmPaymentAsync(
            transactionId,
            new LinePayConfirmRequest(payment.Amount.Amount, "TWD"),
            ct);

        if (confirmResponse.ReturnCode != "0000")
            return Results.BadRequest(new { code = "linepay_confirm_error", message = confirmResponse.ReturnMessage });

        var confirmResult = await dispatcher.Send(
            new ConfirmPaymentCommand(orderId, transactionId.ToString()), ct);
        if (!confirmResult.IsSuccess) return confirmResult.ToHttpResult();

        return Results.Redirect($"{frontendDomain}/orders/{orderId}?payment=success");
    }
}
