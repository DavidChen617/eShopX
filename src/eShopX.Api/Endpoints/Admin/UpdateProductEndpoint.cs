using eShopX.Application.UseCases.Products;

namespace eShopX.Endpoints.Admin;

public sealed class UpdateProductEndpoint : IGroupedEndpoint<AdminProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/{productId:guid}", Handle);
    }

    private static async Task<IResult> Handle(
        Guid productId,
        UpdateProductRequest request,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(
            new UpdateProductCommand(productId, request.Name, request.Description, request.Audience, request.CategoryId),
            ct);
        return result.ToHttpResult();
    }
}

public record UpdateProductRequest(
    string Name,
    string? Description,
    eShopX.Domain.Aggregates.Products.Audience? Audience,
    Guid CategoryId);
