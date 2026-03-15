using eShopX.Application.UseCases.Categories;

namespace eShopX.Endpoints.Categories;

public sealed class GetCategoriesEndpoint : IGroupedEndpoint<CategoriesGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/", Handle)
            .MapToApiVersion(1);
    }

    private static async Task<IResult> Handle(
        IDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetCategoriesQuery(), ct);
        return result.ToHttpResult();
    }
}
