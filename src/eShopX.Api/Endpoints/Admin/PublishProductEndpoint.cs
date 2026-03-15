using eShopX.Application.UseCases.Products;

namespace eShopX.Endpoints.Admin;

public sealed class PublishProductEndpoint : IGroupedEndpoint<AdminProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{productId:guid}/publish", Handle)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid productId,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new PublishProductCommand(productId), ct);
        return result.ToHttpResult();
    }
}
