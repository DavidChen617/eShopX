using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;

namespace Application.UseCases.Categories;

public record UpdateCategoryCommand(Guid CategoryId, string Name) : IRequest<Result>;

public class UpdateCategoryHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateCategoryCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null)
            return Result.NotFound(new Error("category_not_found", "Category not found."));

        category.Rename(command.Name);
        categoryRepository.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
