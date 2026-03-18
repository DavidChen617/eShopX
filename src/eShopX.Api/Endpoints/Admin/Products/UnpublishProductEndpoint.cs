
namespace eShopX.Endpoints.Admin.Products;

public sealed class UnpublishProductEndpoint : IGroupedEndpoint<AdminProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{productId:guid}/unpublish", Handle)
            .Produces(204)
            .Produces<ApiResponse>(404)
            .MapToApiVersion(1);
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
