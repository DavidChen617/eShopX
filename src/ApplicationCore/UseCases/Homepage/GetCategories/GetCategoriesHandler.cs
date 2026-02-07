using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Homepage.GetCategories;

public class GetCategoriesHandler(
    IReadRepository<Category> categoryRepository,
    ICacheService cacheService)
    : IRequestHandler<GetCategoriesQuery, GetCategoriesResponse>
{
    public async Task<GetCategoriesResponse> Handle(GetCategoriesQuery query, CancellationToken cancellationToken = default)
    {
        var parentKey = query.ParentId?.ToString() ?? "root";
        return await cacheService.GetOrSetAsync(
            $"homepage:categories:{parentKey}",
            async ct =>
            {
                var categories = await categoryRepository.QueryAsync(q =>
                        q.Where(c => c.IsActive)
                            .Where(c => c.ParentId == query.ParentId)
                            .OrderBy(c => c.SortOrder)
                            .Select(c => new CategoryItem(c.Id, c.Name, c.Icon, c.Link)),
                    ct);

                return new GetCategoriesResponse(categories.ToList());
            },
            TimeSpan.FromMinutes(2),
            cancellationToken);
    }
}
