using eShopX.Application.UseCases.Products;

namespace eShopX.Endpoints.Products;

public sealed class GetProductByIdEndpoint : IGroupedEndpoint<ProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/{productId:guid}", Handle)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid productId,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetProductByIdQuery(productId), ct);
        return result.ToHttpResult();
    }
}
