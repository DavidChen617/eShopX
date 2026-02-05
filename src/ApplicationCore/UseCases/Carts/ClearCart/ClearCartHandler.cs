namespace ApplicationCore.UseCases.Carts.ClearCart;

public class ClearCartHandler(
    IRepository<Cart> cartRepository,
    IRepository<CartItem> cartItemRepository)
    : IRequestHandler<ClearCartCommand>
{
    public async Task Handle(ClearCartCommand command, CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.FirstOrDefaultAsync(c => c.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Cart), command.UserId);

        var items = await cartItemRepository.ListAsync(i => i.CartId == cart.Id, cancellationToken);
        foreach (var item in items)
        {
            cartItemRepository.Remove(item);
        }

        await cartItemRepository.SaveChangesAsync(cancellationToken);
    }
}