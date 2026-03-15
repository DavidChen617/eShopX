using eShopX.Application.UseCases.Carts;

namespace eShopX.Endpoints.Cart;

public sealed class AddCartItemEndpoint : IGroupedEndpoint<CartGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/items", Handle);
    }

    private static async Task<IResult> Handle(
        AddCartItemRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await dispatcher.Send(new AddCartItemCommand(userId, request.SkuId, request.Quantity), ct);
        return result.ToHttpResult();
    }
}

public record AddCartItemRequest(Guid SkuId, int Quantity);
