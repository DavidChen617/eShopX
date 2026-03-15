using eShopX.Application.UseCases.Carts;

namespace eShopX.Endpoints.Cart;

public sealed class RemoveCartItemEndpoint : IGroupedEndpoint<CartGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/items/{skuId:guid}", Handle)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid skuId,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await dispatcher.Send(new RemoveCartItemCommand(userId, skuId), ct);
        return result.ToHttpResult();
    }
}
