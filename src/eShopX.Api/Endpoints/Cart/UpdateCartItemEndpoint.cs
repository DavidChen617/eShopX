using eShopX.Application.UseCases.Carts;

namespace eShopX.Endpoints.Cart;

public sealed class UpdateCartItemEndpoint : IGroupedEndpoint<CartGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/items/{skuId:guid}", Handle)
            .Produces(204)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid skuId,
        UpdateCartItemRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await dispatcher.Send(new UpdateCartItemQuantityCommand(userId, skuId, request.Quantity), ct);
        return result.ToHttpResult();
    }
}

public record UpdateCartItemRequest(int Quantity);
