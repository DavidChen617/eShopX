using ApplicationCore.UseCases.Homepage.DeleteCategory;

namespace eShopX.Endpoints.Homepage;

public class DeleteCategoryEndpoint : IGroupedEndpoint<HomepageGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapDelete("/categories/{id:guid}", Handle)
            .Produces<DeleteCategoryResponse>()
            .WithDescription("刪除分類")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handle([FromRoute] Guid id, ISender sender)
    {
        var result = await sender.Send(new DeleteCategoryCommand(id));
        return Results.Ok(ApiResponse<DeleteCategoryResponse>.Success(result));
    }
}