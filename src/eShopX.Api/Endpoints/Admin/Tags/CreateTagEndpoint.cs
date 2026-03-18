using Domain.Aggregates.Tags;

namespace eShopX.Endpoints.Admin.Tags;

public sealed class CreateTagEndpoint : IGroupedEndpoint<AdminTagsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/", Handle)
            .Produces<ApiResponse<TagResponse>>(201)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        CreateTagRequest request,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new CreateTagCommand(request.Name, request.Type), ct);
        return result.ToHttpResult();
    }
}

public record CreateTagRequest(string Name, TagType Type);
