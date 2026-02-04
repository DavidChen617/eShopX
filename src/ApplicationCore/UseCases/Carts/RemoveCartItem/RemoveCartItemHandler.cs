namespace ApplicationCore.UseCases.Carts.RemoveCartItem;

public class RemoveCartItemHandler(
    IRepository<Cart> cartRepository,
    IRepository<CartItem> cartItemRepository)
    : IRequestHandler<RemoveCartItemCommand>
{
    public async Task Handle(RemoveCartItemCommand command, CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.FirstOrDefaultAsync(c => c.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Cart), command.UserId);

        var cartItem = await cartItemRepository.FirstOrDefaultAsync(
            i => i.CartId == cart.Id && i.ProductId == command.ProductId,
            cancellationToken)
            ?? throw new NotFoundException(nameof(CartItem), command.ProductId);

        cartItemRepository.Remove(cartItem);
        await cartItemRepository.SaveChangesAsync(cancellationToken);
    }
}
