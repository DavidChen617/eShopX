using ApplicationCore.Entities;
using ApplicationCore.UseCases.Orders;
using ApplicationCore.UseCases.Orders.CreatePaidOrderFromCart;

using eShopX.Common.Exceptions;

using UnitTests.Helpers;

namespace UnitTests.UseCases.Orders;

public class CreatePaidOrderFromCartHandlerTests
{
    [Fact]
    public async Task Handle_EmptyCart_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var orderRepo = new InMemoryRepository<Order>();
        var cartRepo = new InMemoryRepository<Cart>([new Cart { Id = Guid.NewGuid(), UserId = userId }]);
        var cartItemRepo = new InMemoryRepository<CartItem>();
        var productRepo = new InMemoryRepository<Product>();
        var userRepo = new InMemoryRepository<User>([new User { Id = userId }]);

        var handler = new CreatePaidOrderFromCartHandler(
            orderRepo,
            cartRepo,
            cartItemRepo,
            productRepo,
            userRepo);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new CreatePaidOrderFromCartCommand(userId, "Card"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MissingProduct_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId };
        var cartItem = new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductId = Guid.NewGuid(), Quantity = 1 };

        var orderRepo = new InMemoryRepository<Order>();
        var cartRepo = new InMemoryRepository<Cart>([cart]);
        var cartItemRepo = new InMemoryRepository<CartItem>([cartItem]);
        var productRepo = new InMemoryRepository<Product>();
        var userRepo = new InMemoryRepository<User>([new User { Id = userId }]);

        var handler = new CreatePaidOrderFromCartHandler(
            orderRepo,
            cartRepo,
            cartItemRepo,
            productRepo,
            userRepo);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CreatePaidOrderFromCartCommand(userId, "Card"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Success_CreatesOrderReducesStockAndClearsCart()
    {
        var user = new User { Id = Guid.NewGuid(), Name = "User", Phone = "123", Address = "Addr" };
        var cart = new Cart { Id = Guid.NewGuid(), UserId = user.Id };
        var product = new Product { Id = Guid.NewGuid(), Name = "Item", IsActive = true, Price = 10, StockQuantity = 5 };
        var cartItem = new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductId = product.Id, Quantity = 2 };

        var orderRepo = new InMemoryRepository<Order>();
        var cartRepo = new InMemoryRepository<Cart>([cart]);
        var cartItemRepo = new InMemoryRepository<CartItem>([cartItem]);
        var productRepo = new InMemoryRepository<Product>([product]);
        var userRepo = new InMemoryRepository<User>([user]);

        var handler = new CreatePaidOrderFromCartHandler(
            orderRepo,
            cartRepo,
            cartItemRepo,
            productRepo,
            userRepo);

        var response = await handler.Handle(new CreatePaidOrderFromCartCommand(user.Id, "Card"), CancellationToken.None);

        Assert.Equal(OrderStatus.Paid, response.Status);
        Assert.Equal(20, response.TotalAmount);
        Assert.Equal(3, product.StockQuantity);
        Assert.Empty(cartItemRepo.Data);
        Assert.Single(orderRepo.Data);
    }
}