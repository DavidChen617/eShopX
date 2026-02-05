using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Homepage.UpdateCategory;

public class UpdateCategoryHandler(
    IRepository<Category> categoryRepository,
    ICacheService cacheService)
    : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResponse>
{
    public async Task<UpdateCategoryResponse> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Category", command.Id);

        category.Name = command.Name;
        category.Icon = command.Icon;
        category.Link = command.Link;
        category.SortOrder = command.SortOrder;
        category.IsActive = command.IsActive;
        category.ParentId = command.ParentId;
        category.UpdatedAt = DateTime.UtcNow;

        categoryRepository.Update(category);
        await categoryRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:categories", cancellationToken);

        return new UpdateCategoryResponse(true);
    }
}