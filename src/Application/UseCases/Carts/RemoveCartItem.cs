using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Carts;

public record RemoveCartItemCommand(Guid UserId, Guid SkuId) : IRequest<Result>;

public class RemoveCartItemHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork,
    ICacher cacher) : IRequestHandler<RemoveCartItemCommand, Result>
{
    public async Task<Result> Handle(
        RemoveCartItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (cart is null)
            return Result.NotFound(new Error("cart_not_found", "Cart not found."));

        cart.RemoveItem(command.SkuId);
        cartRepository.Update(cart);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacher.RemoveAsync(CartCacheKeys.Cart(command.UserId), cancellationToken);

        return Result.NoContent();
    }
}
