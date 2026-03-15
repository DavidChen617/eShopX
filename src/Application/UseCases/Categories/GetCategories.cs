using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Categories;

public record GetCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryResponse>>>;

public record CategoryResponse(Guid Id, string Name, DateTime CreatedAt);

public class GetCategoriesHandler(
    ICategoryRepository categoryRepository) : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryResponse>>>
{
    public async Task<Result<IReadOnlyList<CategoryResponse>>> Handle(
        GetCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        var response = categories.Select(c => new CategoryResponse(c.Id, c.Name, c.CreatedAt)).ToList();
        return Result<IReadOnlyList<CategoryResponse>>.Ok(response);
    }
}
