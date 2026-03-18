
namespace eShopX.Endpoints.Admin.Sizes;

public sealed class UpdateSizeEndpoint : IGroupedEndpoint<AdminSizesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/{sizeId:guid}", Handle)
            .Produces(204)
            .Produces<ApiResponse>(404)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid sizeId,
        UpdateSizeRequest request,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new UpdateSizeCommand(sizeId, request.Name), ct);
        return result.ToHttpResult();
    }
}

public record UpdateSizeRequest(string Name);
