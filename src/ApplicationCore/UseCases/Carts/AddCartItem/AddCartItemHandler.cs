namespace ApplicationCore.UseCases.Carts.AddCartItem;

public class AddCartItemHandler(
    IRepository<Cart> cartRepository,
    IRepository<CartItem> cartItemRepository,
    IRepository<Product> productRepository)
    : IRequestHandler<AddCartItemCommand, AddCartItemResponse>
{
    public async Task<AddCartItemResponse> Handle(AddCartItemCommand command, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(command.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Product {command.ProductId} does not exist", command.ProductId);

        if (!product.IsActive)
            throw new ValidationException("ProductId", "Product is no longer available");

        if (product.StockQuantity < command.Quantity)
            throw new ValidationException("Quantity", $"Insufficient stock. Available: {product.StockQuantity}");

        var cart = await cartRepository.FirstOrDefaultAsync(c => c.UserId == command.UserId, cancellationToken);

        if (cart == null)
        {
            cart = new Cart { UserId = command.UserId };
            await cartRepository.AddAsync(cart, cancellationToken);
        }

        CartItem? existingItem = null;
        if (cart.Id != Guid.Empty)
        {
            existingItem = await cartItemRepository.FirstOrDefaultAsync(
                i => i.CartId == cart.Id && i.ProductId == command.ProductId,
                cancellationToken);
        }

        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + command.Quantity;
            if (product.StockQuantity < newQuantity)
                throw new ValidationException("Quantity", $"Insufficient stock. Available: {product.StockQuantity}, in cart: {existingItem.Quantity}");

            existingItem.Quantity = newQuantity;
            cartItemRepository.Update(existingItem);
        }
        else
        {
            await cartItemRepository.AddAsync(new CartItem
            {
                CartId = cart.Id,
                ProductId = command.ProductId,
                Quantity = command.Quantity
            }, cancellationToken);
        }

        await cartItemRepository.SaveChangesAsync(cancellationToken);

        var quantity = existingItem?.Quantity ?? command.Quantity;
        return new AddCartItemResponse(
            cart.Id,
            product.Id,
            product.Name,
            quantity,
            product.Price,
            product.Price * quantity
        );
    }
}
