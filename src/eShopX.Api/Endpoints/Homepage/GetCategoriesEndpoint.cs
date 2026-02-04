using ApplicationCore.UseCases.Homepage.GetCategories;

namespace eShopX.Endpoints.Homepage;

public class GetCategoriesEndpoint : IGroupedEndpoint<HomepageGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/categories", Handle)
            .Produces<GetCategoriesResponse>()
            .WithDescription("取得分類入口列表");
    }

    private static async Task<IResult> Handle([FromQuery] Guid? parentId, ISender sender)
    {
        var result = await sender.Send(new GetCategoriesQuery(parentId));
        return Results.Ok(ApiResponse<GetCategoriesResponse>.Success(result));
    }
}
