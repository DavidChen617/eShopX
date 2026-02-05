using ApplicationCore.UseCases.Homepage.CreateCategory;

namespace eShopX.Endpoints.Homepage;

public class CreateCategoryEndpoint : IGroupedEndpoint<HomepageGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/categories", Handle)
            .Accepts<CreateCategoryCommand>(MediaTypeNames.Application.Json)
            .Produces<CreateCategoryResponse>()
            .WithDescription("新增分類")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handle([FromBody] CreateCategoryCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return Results.Ok(ApiResponse<CreateCategoryResponse>.Success(result));
    }
}