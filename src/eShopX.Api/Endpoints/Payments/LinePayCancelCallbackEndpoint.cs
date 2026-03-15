using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using CoreMesh.Result.Http;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Application.UseCases.Payments;
using Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace eShopX.Api.Endpoints.Payments;

public sealed class LinePayCancelCallbackEndpoint : IGroupedEndpoint<PaymentsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/linepay/cancel", Handle);
    }

    private static async Task<IResult> Handle(
        Guid orderId,
        IDispatcher dispatcher,
        IPaymentRepository paymentRepository,
        IOptions<LinePayOptions> linePayOptions,
        CancellationToken ct)
    {
        var payment = await paymentRepository.GetByOrderIdAsync(orderId, ct);
        if (payment is not null)
        {
            var result = await dispatcher.Send(new MarkPaymentAsFailedCommand(payment.Id), ct);
            if (!result.IsSuccess) return result.ToHttpResult();
        }

        var frontendUrl = linePayOptions.Value.FrontendBaseUrl;
        return Results.Redirect($"{frontendUrl}/orders/{orderId}?payment=cancelled");
    }
}
