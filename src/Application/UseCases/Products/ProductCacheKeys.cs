using eShopX.Domain.Aggregates.Products;

namespace eShopX.Application.UseCases.Products;

internal static class ProductCacheKeys
{
    public static string Product(Guid productId) => $"product:{productId}";

    public static string ProductList(Guid? categoryId, Audience? audience, bool? isActive, int page, int pageSize)
        => $"products:{categoryId}:{audience}:{isActive}:{page}:{pageSize}";
}
