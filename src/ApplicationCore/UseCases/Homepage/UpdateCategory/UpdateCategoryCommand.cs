namespace ApplicationCore.UseCases.Homepage.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Icon,
    string Link,
    int SortOrder,
    bool IsActive,
    Guid? ParentId) : IRequest<UpdateCategoryResponse>;
