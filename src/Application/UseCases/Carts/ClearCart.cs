using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;

namespace Application.UseCases.Carts;

public record ClearCartCommand(Guid UserId) : IRequest<Result>;

public class ClearCartHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork,
    ICacher cacher) : IRequestHandler<ClearCartCommand, Result>
{
    public async Task<Result> Handle(
        ClearCartCommand command,
        CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (cart is null)
            return Result.NotFound(new Error("cart_not_found", "Cart not found."));

        cart.Clear();
        cartRepository.Update(cart);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacher.RemoveAsync(CartCacheKeys.Cart(command.UserId), cancellationToken);

        return Result.NoContent();
    }
}
