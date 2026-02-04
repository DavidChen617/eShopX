namespace ApplicationCore.UseCases.Homepage.GetCategories;

public record GetCategoriesQuery(Guid? ParentId = null) : IRequest<GetCategoriesResponse>;
