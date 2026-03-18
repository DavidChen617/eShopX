
namespace eShopX.Endpoints.Admin.Tags;

public sealed class UpdateTagEndpoint : IGroupedEndpoint<AdminTagsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/{tagId:guid}", Handle)
            .Produces(204)
            .Produces<ApiResponse>(404)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid tagId,
        UpdateTagRequest request,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new UpdateTagCommand(tagId, request.Name), ct);
        return result.ToHttpResult();
    }
}

public record UpdateTagRequest(string Name);
