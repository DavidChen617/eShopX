using ApplicationCore.Interfaces;

namespace ApplicationCore.UseCases.Homepage.DeleteCategory;

public class DeleteCategoryHandler(
    IRepository<Category> categoryRepository,
    ICacheService cacheService)
    : IRequestHandler<DeleteCategoryCommand, DeleteCategoryResponse>
{
    public async Task<DeleteCategoryResponse> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Category", command.Id);

        categoryRepository.Remove(category);
        await categoryRepository.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("homepage:categories", cancellationToken);

        return new DeleteCategoryResponse(true);
    }
}