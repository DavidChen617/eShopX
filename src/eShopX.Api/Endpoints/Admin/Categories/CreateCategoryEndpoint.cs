
namespace eShopX.Endpoints.Admin.Categories;

public sealed class CreateCategoryEndpoint : IGroupedEndpoint<AdminCategoriesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("/", Handle)
            .Produces<ApiResponse<CategoryResponse>>(201)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        CreateCategoryRequest request,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new CreateCategoryCommand(request.Name), ct);
        return result.ToHttpResult();
    }
}

public record CreateCategoryRequest(string Name);
