using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Domain.Aggregates.Categories;

namespace Application.UseCases.Categories;

public record CreateCategoryCommand(string Name) : IRequest<Result<CategoryResponse>>;

public class CreateCategoryHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateCategoryCommand, Result<CategoryResponse>>
{
    public async Task<Result<CategoryResponse>> Handle(
        CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var category = Category.Create(command.Name);
        await categoryRepository.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CategoryResponse>.Created(new CategoryResponse(category.Id, category.Name, category.CreatedAt));
    }
}
