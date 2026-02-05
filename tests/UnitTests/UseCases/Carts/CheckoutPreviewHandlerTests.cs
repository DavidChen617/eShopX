using ApplicationCore.Entities;
using ApplicationCore.UseCases.Carts.CheckoutPreview;

using UnitTests.Helpers;

namespace UnitTests.UseCases.Carts;

public class CheckoutPreviewHandlerTests
{
    [Fact]
    public async Task Handle_EmptyCart_ReturnsEmptyResponse()
    {
        var cartRepo = new InMemoryRepository<Cart>();
        var handler = new CheckoutPreviewHandler(cartRepo);

        var response = await handler.Handle(new CheckoutPreviewQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalAmount);
        Assert.False(response.HasUnavailableItems);
    }

    [Fact]
    public async Task Handle_MixedAvailability_ComputesTotalsAndFlags()
    {
        var userId = Guid.NewGuid();
        var available = new Product { Id = Guid.NewGuid(), Name = "A", Price = 10, StockQuantity = 5, IsActive = true };
        var unavailable = new Product { Id = Guid.NewGuid(), Name = "B", Price = 20, StockQuantity = 0, IsActive = true };

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Items =
            [
                new CartItem { Id = Guid.NewGuid(), ProductId = available.Id, Quantity = 2, Product = available },
                new CartItem { Id = Guid.NewGuid(), ProductId = unavailable.Id, Quantity = 1, Product = unavailable }
            ]
        };

        var cartRepo = new InMemoryRepository<Cart>([cart]);
        var handler = new CheckoutPreviewHandler(cartRepo);

        var response = await handler.Handle(new CheckoutPreviewQuery(userId), CancellationToken.None);

        Assert.Equal(2, response.Items.Count);
        Assert.Equal(20, response.TotalAmount);
        Assert.True(response.HasUnavailableItems);
    }
}