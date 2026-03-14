namespace eShopX.Application.UseCases.Carts;

internal static class CartCacheKeys
{
    public static string Cart(Guid userId) => $"cart:{userId}";
}
