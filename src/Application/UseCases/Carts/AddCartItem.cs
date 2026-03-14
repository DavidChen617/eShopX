using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using CoreMesh.Validation.Abstractions;
using CoreMesh.Validation.Abstractions.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Carts;

namespace eShopX.Application.UseCases.Carts;

public record AddCartItemCommand(
    Guid UserId,
    Guid SkuId,
    int Quantity) : IRequest<Result>, IValidatable<AddCartItemCommand>
{
    public void ConfigureValidateRules(IValidationBuilder<AddCartItemCommand> builder)
    {
        builder.For(x => x.Quantity)
            .GreaterThan(0, "Quantity must be greater than zero.");
    }
}

public class AddCartItemHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork,
    ICacher cacher,
    IValidator validator) : IRequestHandler<AddCartItemCommand, Result>
{
    public async Task<Result> Handle(
        AddCartItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(command);
        if (!validation.IsValid)
            return Result.Invalid(validation.Errors);

        var cart = await cartRepository.GetByUserIdAsync(command.UserId, cancellationToken);

        if (cart is null)
        {
            cart = Cart.Create(command.UserId);
            cart.AddItem(command.SkuId, command.Quantity);
            await cartRepository.AddAsync(cart, cancellationToken);
        }
        else
        {
            cart.AddItem(command.SkuId, command.Quantity);
            cartRepository.Update(cart);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacher.RemoveAsync(CartCacheKeys.Cart(command.UserId), cancellationToken);
        return Result.NoContent();
    }
}
