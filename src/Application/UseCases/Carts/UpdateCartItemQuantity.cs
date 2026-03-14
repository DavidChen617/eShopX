using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using CoreMesh.Validation.Abstractions;
using CoreMesh.Validation.Abstractions.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Carts;

public record UpdateCartItemQuantityCommand(
    Guid UserId,
    Guid SkuId,
    int Quantity) : IRequest<Result>, IValidatable<UpdateCartItemQuantityCommand>
{
    public void ConfigureValidateRules(IValidationBuilder<UpdateCartItemQuantityCommand> builder)
    {
        builder.For(x => x.Quantity)
            .GreaterThan(0, "Quantity must be greater than zero.");
    }
}

public class UpdateCartItemQuantityHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork,
    ICacher cacher,
    IValidator validator) : IRequestHandler<UpdateCartItemQuantityCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCartItemQuantityCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(command);
        if (!validation.IsValid)
            return Result.Invalid(validation.Errors);

        var cart = await cartRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (cart is null)
            return Result.NotFound(new Error("cart_not_found", "Cart not found."));

        cart.UpdateQuantity(command.SkuId, command.Quantity);
        cartRepository.Update(cart);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacher.RemoveAsync(CartCacheKeys.Cart(command.UserId), cancellationToken);

        return Result.NoContent();
    }
}
