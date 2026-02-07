using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Homepage.CreateCategory;

public class CreateCategoryHandler(
    IRepository<Category> categoryRepository,
    ICacheService cacheService)
    : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Icon = command.Icon,
            Link = command.Link,
            SortOrder = command.SortOrder,
            ParentId = command.ParentId,
            IsActive = true,
            UpdatedAt = DateTime.UtcNow
        };

        await categoryRepository.AddAsync(category, cancellationToken);
        await categoryRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:categories", cancellationToken);

        return new CreateCategoryResponse(category.Id);
    }
}
