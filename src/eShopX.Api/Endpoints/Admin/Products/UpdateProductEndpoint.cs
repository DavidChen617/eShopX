using Domain.Aggregates.Products;

namespace eShopX.Endpoints.Admin.Products;

public sealed class UpdateProductEndpoint : IGroupedEndpoint<AdminProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/{productId:guid}", Handle)
            .Produces(204)
            .Produces<ApiResponse>(404)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid productId,
        UpdateProductRequest request,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(
            new UpdateProductCommand(
                productId,
                request.Name,
                request.Description,
                request.Audience,
                request.CategoryId,
                request.TagIds,
                request.Variants),
            ct);
        return result.ToHttpResult();
    }
}

public record UpdateProductRequest(
    string Name,
    string? Description,
    Audience? Audience,
    Guid CategoryId,
    IReadOnlyList<Guid>? TagIds,
    IReadOnlyList<UpdateVariantRequest>? Variants);
