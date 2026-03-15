using eShopX.Application.UseCases.Products;

namespace eShopX.Endpoints.Admin;

public sealed class CreateProductEndpoint : IGroupedEndpoint<AdminProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/", Handle);
    }

    private static async Task<IResult> Handle(
        CreateProductCommand command,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(command, ct);
        return result.ToHttpResult();
    }
}
