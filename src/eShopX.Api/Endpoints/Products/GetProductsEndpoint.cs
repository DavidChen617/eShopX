using Domain.Aggregates.Products;
using Microsoft.AspNetCore.Mvc;

namespace eShopX.Endpoints.Products;

public sealed class GetProductsEndpoint : IGroupedEndpoint<ProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/", Handle)
            .Produces<ApiResponse<GetProductsResponse>>(200)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        [FromQuery] Guid? categoryId,
        [FromQuery] Audience? audience,
        [FromQuery] bool? isActive,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(
            new GetProductsQuery(categoryId, audience, isActive, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize),
            ct);
        return result.ToHttpResult();
    }
}
