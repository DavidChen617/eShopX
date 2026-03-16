using eShopX.Application.UseCases.Carts;

namespace eShopX.Endpoints.Cart;

public sealed class GetCartEndpoint : IGroupedEndpoint<CartGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/", Handle)
            .Produces<ApiResponse<CartResponse>>(200)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await dispatcher.Send(new GetCartQuery(userId), ct);
        return result.ToHttpResult();
    }
}
