namespace ApplicationCore.UseCases.Orders.CreatePaidOrderFromCart;

public class CreatePaidOrderFromCartHandler(
    IRepository<Order> orderRepository,
    IRepository<Cart> cartRepository,
    IRepository<CartItem> cartItemRepository,
    IRepository<Product> productRepository,
    IRepository<User> userRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePaidOrderFromCartCommand, CreatePaidOrderFromCartResponse>
{
    public async Task<CreatePaidOrderFromCartResponse> Handle(
        CreatePaidOrderFromCartCommand command,
        CancellationToken cancellationToken = default)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken)
                ?? throw new NotFoundException("User", command.UserId);

            var cart = await cartRepository.FirstOrDefaultAsync(c => c.UserId == command.UserId, cancellationToken)
                ?? throw new NotFoundException(nameof(Cart), command.UserId);

            var cartItems = await cartItemRepository.ListAsync(i => i.CartId == cart.Id, cancellationToken);
            if (cartItems.Count == 0)
            {
                throw new ValidationException("Cart", "Cart is empty");
            }

            var productIds = cartItems.Select(i => i.ProductId).ToHashSet();
            var products = await productRepository.ListAsync(p => productIds.Contains(p.Id), cancellationToken);

            if (products.Count != cartItems.Count)
            {
                var productIdSet = products.Select(p => p.Id).ToHashSet();
                var missingIds = productIds.Except(productIdSet);
                throw new NotFoundException("Product", string.Join(",", missingIds));
            }

            var orderItems = new List<OrderItem>();
            foreach (var product in products)
            {
                if (!product.IsActive)
                {
                    throw new ConflictException($"Product {product.Id} not active!");
                }

                var cartItem = cartItems.First(x => x.ProductId == product.Id);
                if (cartItem.Quantity <= 0)
                {
                    throw new ValidationException("Quantity", "Quantity must be greater than zero");
                }

                if (cartItem.Quantity > product.StockQuantity)
                {
                    throw new ConflictException($"Product {product.Id} not stocked!");
                }

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = cartItem.Quantity,
                    ProductName = product.Name,
                    UnitPrice = product.Price
                });

                product.StockQuantity -= cartItem.Quantity;
            }

            var paidAt = DateTime.UtcNow;
            var order = await orderRepository.AddAsync(
                new Order
                {
                    TotalAmount = orderItems.Sum(o => o.UnitPrice * o.Quantity),
                    ShippingAddress = user.Address ?? string.Empty,
                    ShippingPhone = user.Phone ?? string.Empty,
                    ShippingName = user.Name ?? string.Empty,
                    Status = OrderStatus.Paid,
                    UserId = user.Id,
                    Items = orderItems,
                    PaymentMethod = command.PaymentMethod,
                    PaidAt = paidAt
                },
                cancellationToken);

            productRepository.UpdateRange(products);
            cartItemRepository.RemoveRange(cartItems);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return new CreatePaidOrderFromCartResponse(
                order.Id,
                order.Status,
                order.TotalAmount,
                order.PaymentMethod,
                paidAt);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
