using eShopX.Application.UseCases.Categories;

namespace eShopX.Endpoints.Admin.Categories;

public sealed class UpdateCategoryEndpoint : IGroupedEndpoint<AdminCategoriesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPut("/{categoryId:guid}", Handle)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        Guid categoryId,
        UpdateCategoryRequest request,
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new UpdateCategoryCommand(categoryId, request.Name), ct);
        return result.ToHttpResult();
    }
}

public record UpdateCategoryRequest(string Name);
