using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;

namespace Application.UseCases.Carts;

public record GetCartQuery(Guid UserId) : IRequest<Result<CartResponse>>;

public record CartItemResponse(
    Guid ItemId,
    Guid SkuId,
    int Quantity,
    string ProductName,
    string Color,
    string? SizeName,
    decimal UnitPrice,
    string? PrimaryImageUrl);

public record CartResponse(
    Guid? CartId,
    Guid UserId,
    IReadOnlyList<CartItemResponse> Items);

public class GetCartHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ISizeRepository sizeRepository,
    ICacher cacher) : IRequestHandler<GetCartQuery, Result<CartResponse>>
{
    public async Task<Result<CartResponse>> Handle(
        GetCartQuery query,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = CartCacheKeys.Cart(query.UserId);
        var cached = await cacher.GetAsync<CartResponse>(cacheKey, cancellationToken);
        if (cached is not null)
            return Result<CartResponse>.Ok(cached);

        var cart = await cartRepository.GetByUserIdAsync(query.UserId, cancellationToken);
        if (cart is null)
            return Result<CartResponse>.Ok(new CartResponse(null, query.UserId, []));

        var items = new List<CartItemResponse>();
        foreach (var item in cart.Items)
        {
            var skuDetails = await productRepository.GetSkuDetailsAsync(item.SkuId, cancellationToken);
            if (skuDetails is null) continue;

            string? sizeName = null;
            if (skuDetails.Sku.SizeId.HasValue)
            {
                var size = await sizeRepository.GetByIdAsync(skuDetails.Sku.SizeId.Value, cancellationToken);
                sizeName = size?.Name;
            }

            var primaryImage = skuDetails.Variant.Images.FirstOrDefault(i => i.IsPrimary)
                               ?? skuDetails.Variant.Images.FirstOrDefault();

            items.Add(new CartItemResponse(
                item.Id,
                item.SkuId,
                item.Quantity,
                skuDetails.Product.Name,
                skuDetails.Variant.Color,
                sizeName,
                skuDetails.Sku.Price.Amount,
                primaryImage?.Url));
        }

        var response = new CartResponse(cart.Id, cart.UserId, items);
        await cacher.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10), cancellationToken);

        return Result<CartResponse>.Ok(response);
    }
}
