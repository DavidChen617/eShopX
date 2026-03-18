
namespace eShopX.Endpoints.Admin.Tags;

public sealed class DeleteTagEndpoint : IGroupedEndpoint<AdminTagsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/{tagId:guid}", Handle)
            .Produces(204)
            .Produces<ApiResponse>(404)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid tagId,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new DeleteTagCommand(tagId), ct);
        return result.ToHttpResult();
    }
}
