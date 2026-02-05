namespace ApplicationCore.UseCases.Homepage.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest<DeleteCategoryResponse>;