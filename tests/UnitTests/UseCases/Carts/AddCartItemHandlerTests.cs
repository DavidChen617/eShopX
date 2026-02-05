using ApplicationCore.Entities;
using ApplicationCore.UseCases.Carts.AddCartItem;

using eShopX.Common.Exceptions;

using UnitTests.Helpers;

namespace UnitTests.UseCases.Carts;

public class AddCartItemHandlerTests
{
    [Fact]
    public async Task Handle_ProductInactive_ThrowsValidationException()
    {
        var product = new Product { Id = Guid.NewGuid(), IsActive = false, StockQuantity = 10, Price = 100 };
        var productRepo = new InMemoryRepository<Product>([product]);
        var cartRepo = new InMemoryRepository<Cart>();
        var cartItemRepo = new InMemoryRepository<CartItem>();

        var handler = new AddCartItemHandler(cartRepo, cartItemRepo, productRepo);

        var command = new AddCartItemCommand(Guid.NewGuid(), product.Id, 1);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InsufficientStock_ThrowsValidationException()
    {
        var product = new Product { Id = Guid.NewGuid(), IsActive = true, StockQuantity = 1, Price = 100 };
        var productRepo = new InMemoryRepository<Product>([product]);
        var cartRepo = new InMemoryRepository<Cart>();
        var cartItemRepo = new InMemoryRepository<CartItem>();

        var handler = new AddCartItemHandler(cartRepo, cartItemRepo, productRepo);

        var command = new AddCartItemCommand(Guid.NewGuid(), product.Id, 2);

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NewCart_AddsItemAndReturnsResponse()
    {
        var userId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), IsActive = true, StockQuantity = 10, Price = 50, Name = "Item" };
        var productRepo = new InMemoryRepository<Product>([product]);
        var cartRepo = new InMemoryRepository<Cart>();
        var cartItemRepo = new InMemoryRepository<CartItem>();

        var handler = new AddCartItemHandler(cartRepo, cartItemRepo, productRepo);

        var response = await handler.Handle(new AddCartItemCommand(userId, product.Id, 2), CancellationToken.None);

        Assert.Equal(product.Id, response.ProductId);
        Assert.Equal(2, response.Quantity);
        Assert.Equal(100, response.Subtotal);
        Assert.Single(cartRepo.Data);
        Assert.Single(cartRepo.Data[0].Items);
    }

    [Fact]
    public async Task Handle_ExistingCartItem_IncrementsQuantity()
    {
        var userId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), IsActive = true, StockQuantity = 10, Price = 30, Name = "Item" };
        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId };
        var existingItem = new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductId = product.Id, Quantity = 3 };

        var productRepo = new InMemoryRepository<Product>([product]);
        var cartRepo = new InMemoryRepository<Cart>([cart]);
        var cartItemRepo = new InMemoryRepository<CartItem>([existingItem]);

        var handler = new AddCartItemHandler(cartRepo, cartItemRepo, productRepo);

        var response = await handler.Handle(new AddCartItemCommand(userId, product.Id, 2), CancellationToken.None);

        Assert.Equal(5, response.Quantity);
        Assert.Equal(150, response.Subtotal);
        Assert.Equal(5, existingItem.Quantity);
    }
}