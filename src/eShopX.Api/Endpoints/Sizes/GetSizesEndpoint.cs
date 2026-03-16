using eShopX.Application.UseCases.Sizes;

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
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetSizesQuery(), ct);
        return result.ToHttpResult();
    }
}
