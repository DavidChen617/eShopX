
namespace eShopX.Endpoints.Admin.Categories;

public sealed class DeleteCategoryEndpoint : IGroupedEndpoint<AdminCategoriesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/{categoryId:guid}", Handle)
            .Produces(204)
            .Produces<ApiResponse>(404)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid categoryId,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new DeleteCategoryCommand(categoryId), ct);
        return result.ToHttpResult();
    }
}
