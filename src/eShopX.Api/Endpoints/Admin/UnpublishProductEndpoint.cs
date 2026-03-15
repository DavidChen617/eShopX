using eShopX.Application.UseCases.Products;

namespace eShopX.Endpoints.Admin;

public sealed class UnpublishProductEndpoint : IGroupedEndpoint<AdminProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{productId:guid}/unpublish", Handle);
    }

    private static async Task<IResult> Handle(
        Guid productId,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new UnpublishProductCommand(productId), ct);
        return result.ToHttpResult();
    }
}
