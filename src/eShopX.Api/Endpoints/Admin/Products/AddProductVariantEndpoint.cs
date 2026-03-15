using eShopX.Application.UseCases.Products;

namespace eShopX.Endpoints.Admin;

public sealed class AddProductVariantEndpoint : IGroupedEndpoint<AdminProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/{productId:guid}/variants", Handle)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid productId,
        AddVariantRequest request,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(
            new AddProductVariantCommand(productId, request.Color, request.Skus, request.Images),
            ct);
        return result.ToHttpResult();
    }
}

public record AddVariantRequest(
    string Color,
    IReadOnlyList<SkuRequest> Skus,
    IReadOnlyList<ImageRequest>? Images);
