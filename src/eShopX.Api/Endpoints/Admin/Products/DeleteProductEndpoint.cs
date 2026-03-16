using eShopX.Application.UseCases.Products;

namespace eShopX.Endpoints.Admin.Products;

public sealed class DeleteProductEndpoint : IGroupedEndpoint<AdminProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/{productId:guid}", Handle)
            .Produces(204)
            .Produces<ApiResponse>(404)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid productId,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new DeleteProductCommand(productId), ct);
        return result.ToHttpResult();
    }
}
