using eShopX.Application.UseCases.Carts;

namespace eShopX.Endpoints.Cart;

public sealed class ClearCartEndpoint : IGroupedEndpoint<CartGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/", Handle);
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await dispatcher.Send(new ClearCartCommand(userId), ct);
        return result.ToHttpResult();
    }
}
