using ApplicationCore.Entities;
using ApplicationCore.UseCases.Carts.GetCart;
using UnitTests.Helpers;

namespace UnitTests.UseCases.Carts;

public class GetCartHandlerTests
{
    [Fact]
    public async Task Handle_NoCart_ReturnsEmptyResponse()
    {
        var cartRepo = new InMemoryRepository<Cart>();
        var handler = new GetCartHandler(cartRepo);

        var response = await handler.Handle(new GetCartQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalAmount);
        Assert.Equal(0, response.TotalItems);
    }

    [Fact]
    public async Task Handle_CartWithItems_ReturnsTotals()
    {
        var userId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), Name = "Item", Price = 20, StockQuantity = 5 };
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Items =
            [
                new CartItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Quantity = 2,
                    Product = product
                }
            ]
        };

        var cartRepo = new InMemoryRepository<Cart>([cart]);
        var handler = new GetCartHandler(cartRepo);

        var response = await handler.Handle(new GetCartQuery(userId), CancellationToken.None);

        Assert.Single(response.Items);
        Assert.Equal(40, response.TotalAmount);
        Assert.Equal(2, response.TotalItems);
    }
}
