using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Mapper;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Carts;

namespace eShopX.Application.UseCases.Carts;

public record GetCartQuery(Guid UserId) : IRequest<Result<CartResponse>>;

public record CartItemResponse(Guid ItemId, Guid SkuId, int Quantity)
    : IMapFrom<CartItem, CartItemResponse>
{
    public CartItemResponse MapFrom(CartItem source) =>
        new(source.Id, source.SkuId, source.Quantity);
}

public record CartResponse(
    Guid CartId,
    Guid UserId,
    IReadOnlyList<CartItemResponse> Items) : IMapFrom<Cart, CartResponse>
{
    public CartResponse MapFrom(Cart source) => new(
        source.Id,
        source.UserId,
        source.Items.Select(i => new CartItemResponse(i.Id, i.SkuId, i.Quantity)).ToList()
    );
}

public class GetCartHandler(
    ICartRepository cartRepository,
    ICacher cacher,
    IMapper mapper) : IRequestHandler<GetCartQuery, Result<CartResponse>>
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
            return Result<CartResponse>.NotFound(new Error("cart_not_found", "Cart not found."));

        var response = mapper.Map<Cart, CartResponse>(cart);
        await cacher.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10), cancellationToken);

        return Result<CartResponse>.Ok(response);
    }
}
