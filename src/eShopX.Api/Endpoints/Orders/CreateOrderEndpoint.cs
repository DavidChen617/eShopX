using Application.UseCases.Orders;
using Domain.Aggregates.Payments;

namespace eShopX.Endpoints.Orders;

public sealed class CreateOrderEndpoint : IGroupedEndpoint<OrdersGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/", Handle)
            .Produces<ApiResponse<CreateOrderResponse>>(201)
            .Produces<ApiResponse>(400)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        CreateOrderRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await dispatcher.Send(
            new CreateOrderCommand(userId, request.PaymentMethod, request.ReceiverName, request.ReceiverPhone),
            ct);
        return result.ToHttpResult();
    }
}

public record CreateOrderRequest(
    PaymentMethod PaymentMethod,
    string ReceiverName,
    string ReceiverPhone);
