namespace ApplicationCore.UseCases.Homepage.GetRecommendProducts;

public record GetRecommendProductsQuery(
    string RecommendType = "homepage",
    int? Page = null,
    int? PageSize = null) : IRequest<GetRecommendProductsResponse>;