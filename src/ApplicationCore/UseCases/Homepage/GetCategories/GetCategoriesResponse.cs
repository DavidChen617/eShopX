namespace ApplicationCore.UseCases.Homepage.GetCategories;

public record GetCategoriesResponse(List<CategoryItem> Items);

public record CategoryItem(
    Guid Id,
    string Name,
    string Icon,
    string Link);
