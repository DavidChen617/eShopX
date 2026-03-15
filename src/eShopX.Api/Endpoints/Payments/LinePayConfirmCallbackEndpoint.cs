using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using CoreMesh.Result.Http;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Application.UseCases.Orders;
using eShopX.Application.UseCases.Payments;
using Infrastructure.Options;
using Infrastructure.Payments.Line;
using Infrastructure.Payments.Line.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace eShopX.Api.Endpoints.Payments;

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
        IOptions<LinePayOptions> linePayOptions,
        CancellationToken ct)
    {
        var payment = await paymentRepository.GetByOrderIdAsync(orderId, ct);
        if (payment is null)
            return Results.NotFound(new { code = "payment_not_found" });

        var confirmResponse = await linePayService.ConfirmPaymentAsync(
            transactionId,
            new LinePayConfirmRequest(payment.Amount.Amount, "TWD"),
            ct);

        if (confirmResponse.ReturnCode != "0000")
            return Results.BadRequest(new { code = "linepay_confirm_error", message = confirmResponse.ReturnMessage });

        var markPaidResult = await dispatcher.Send(
            new MarkPaymentAsPaidCommand(payment.Id, transactionId.ToString()), ct);
        if (!markPaidResult.IsSuccess) return markPaidResult.ToHttpResult();

        var markOrderPaidResult = await dispatcher.Send(new MarkOrderAsPaidCommand(orderId), ct);
        if (!markOrderPaidResult.IsSuccess) return markOrderPaidResult.ToHttpResult();

        var frontendUrl = linePayOptions.Value.FrontendBaseUrl;
        return Results.Redirect($"{frontendUrl}/orders/{orderId}?payment=success");
    }
}
