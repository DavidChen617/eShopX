using ApplicationCore.UseCases.Homepage.UpdateCategory;

namespace eShopX.Endpoints.Homepage;

public class UpdateCategoryEndpoint : IGroupedEndpoint<HomepageGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/categories/{id:guid}", Handle)
            .Accepts<UpdateCategoryRequest>(MediaTypeNames.Application.Json)
            .Produces<UpdateCategoryResponse>()
            .WithDescription("更新分類")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateCategoryRequest request,
        ISender sender)
    {
        var command = new UpdateCategoryCommand(
            id,
            request.Name,
            request.Icon,
            request.Link,
            request.SortOrder,
            request.IsActive,
            request.ParentId);
        var result = await sender.Send(command);
        return Results.Ok(ApiResponse<UpdateCategoryResponse>.Success(result));
    }
}

public record UpdateCategoryRequest(
    string Name,
    string Icon,
    string Link,
    int SortOrder,
    bool IsActive,
    Guid? ParentId);