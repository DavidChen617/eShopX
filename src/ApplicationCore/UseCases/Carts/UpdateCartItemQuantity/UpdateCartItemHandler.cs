namespace ApplicationCore.UseCases.Carts.UpdateCartItemQuantity;

public class UpdateCartItemHandler(
    IRepository<Cart> cartRepository,
    IRepository<CartItem> cartItemRepository,
    IRepository<Product> productRepository)
    : IRequestHandler<UpdateCartItemCommand, UpdateCartItemResponse>
{
    public async Task<UpdateCartItemResponse> Handle(UpdateCartItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.FirstOrDefaultAsync(c => c.UserId == command.UserId, cancellationToken)
                   ?? throw new NotFoundException(nameof(Cart), command.UserId);

        var cartItem = await cartItemRepository.FirstOrDefaultAsync(
                           i => i.CartId == cart.Id && i.ProductId == command.ProductId,
                           cancellationToken)
                       ?? throw new NotFoundException(nameof(CartItem), command.ProductId);

        var product = await productRepository.GetByIdAsync(command.ProductId, cancellationToken)
                      ?? throw new NotFoundException(nameof(Product), command.ProductId);

        if (product.StockQuantity < command.Quantity)
            throw new ValidationException("Quantity", $"Insufficient stock. Available: {product.StockQuantity}");

        cartItem.Quantity = command.Quantity;
        cartItemRepository.Update(cartItem);
        await cartItemRepository.SaveChangesAsync(cancellationToken);

        return new UpdateCartItemResponse(
            product.Id,
            product.Name,
            command.Quantity,
            product.Price,
            product.Price * command.Quantity
        );
    }
}
