
namespace eShopX.Endpoints.Admin.Sizes;

public sealed class DeleteSizeEndpoint : IGroupedEndpoint<AdminSizesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/{sizeId:guid}", Handle)
            .Produces(204)
            .Produces<ApiResponse>(404)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid sizeId,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new DeleteSizeCommand(sizeId), ct);
        return result.ToHttpResult();
    }
}
