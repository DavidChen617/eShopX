using Application.UseCases.Tags;

namespace eShopX.Endpoints.Tags;

public sealed class GetTagsEndpoint : IGroupedEndpoint<TagsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/", Handle)
            .Produces<ApiResponse<IReadOnlyList<TagResponse>>>(200)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetTagsQuery(), ct);
        return result.ToHttpResult();
    }
}
