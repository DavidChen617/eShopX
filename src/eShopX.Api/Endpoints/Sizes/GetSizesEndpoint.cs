using eShopX.Application.UseCases.Sizes;
using eShopX.Domain.Aggregates.Sizes;
using Microsoft.AspNetCore.Mvc;

namespace eShopX.Endpoints.Sizes;

public sealed class GetSizesEndpoint : IGroupedEndpoint<SizesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/", Handle)
            .Produces<ApiResponse<IReadOnlyList<SizeResponse>>>(200)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        [FromQuery] SizeType? type,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetSizesQuery(type), ct);
        return result.ToHttpResult();
    }
}
