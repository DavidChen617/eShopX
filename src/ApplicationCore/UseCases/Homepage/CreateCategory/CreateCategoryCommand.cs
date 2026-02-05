namespace ApplicationCore.UseCases.Homepage.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string Icon,
    string Link,
    int SortOrder,
    Guid? ParentId) : IRequest<CreateCategoryResponse>;