namespace ApplicationCore.UseCases.Orders.CreateOrder;

public class CreateOrderHandler(
    IRepository<Order> orderRepository,
    IRepository<Product> productRepository,
    IRepository<User> userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        const int maxRetries = 5;
        
        for (int attempt = 0; attempt < maxRetries; ++attempt)
        {
            try
            {
                return await CreateOrderInternalAsync(command, cancellationToken);
            }
            catch (ConcurrencyException) when(attempt < maxRetries - 1)
            {
                // Concurrency conflict, wait and try again                                                                                                             
                await Task.Delay(Random.Shared.Next(10, 50), cancellationToken);
            }
        }
        
        throw new ConflictException("系統繁忙，請稍後再試");
    }
    
    private async Task<CreateOrderResponse> CreateOrderInternalAsync(                                                                                          
        CreateOrderCommand command,                                                                                                                            
        CancellationToken cancellationToken)                                                                                                                   
    {                                                                                                                                                          
        return await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var (userId, shippingName, shippingAddress, shippingPhone, items) =
                command;

            if (items.Count == 0)
            {
                throw new ValidationException("items", "Order items must have at least one item");
            }

            var user = await userRepository.GetByIdAsync(userId, ct);

            if (user is null)
            {
                throw new NotFoundException("User", userId);
            }

            HashSet<Guid> productHasSet = new();

            foreach (var item in items)
            {
                if (!productHasSet.Add(item.ProductId))
                {
                    throw new ConflictException($"Product with id {item.ProductId} already exists!");
                }

                if (item.Quantity <= 0)
                {
                    throw new ValidationException(nameof(item.Quantity), "Quantity must be greater than zero");
                }
            }

            var products =
                await productRepository.ListAsync(p => productHasSet.Contains(p.Id), ct);

            if (products.Count != items.Count)
            {
                var productIds = products.Select(p => p.Id).ToHashSet();
                var missingIds = productHasSet.Except(productIds);
                throw new NotFoundException("Product", string.Join(",", missingIds));
            }

            List<OrderItem> orderItems = new();

            foreach (var product in products)
            {
                if (!product.IsActive)
                {
                    throw new ConflictException($"Product {product.Id} not active!");
                }

                var incoming = items.First(x => x.ProductId == product.Id);

                if (incoming.Quantity > product.StockQuantity)
                {
                    throw new ConflictException($"Product {product.Id} not stocked!");
                }

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = incoming.Quantity,
                    ProductName = product.Name,
                    UnitPrice = product.Price
                });

                product.StockQuantity -= incoming.Quantity;
            }

            var order = await orderRepository.AddAsync(
                new Order
                {
                    TotalAmount = orderItems.Sum(o => o.UnitPrice * o.Quantity),
                    ShippingAddress = shippingAddress,
                    ShippingPhone = shippingPhone,
                    ShippingName = shippingName,
                    Status = OrderStatus.Paid,
                    UserId = userId,
                    Items = orderItems,
                    PaymentMethod = "Manual",
                    PaidAt = DateTime.UtcNow
                }, ct);

            productRepository.UpdateRange(products);
            await unitOfWork.SaveChangesAsync(ct);

            return mapper.Map<Order, CreateOrderResponse>(order);
        }, cancellationToken);
    }
}
