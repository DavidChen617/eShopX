using eShopX.Domain.Exceptions;

namespace eShopX.Domain.Aggregates.Carts;

public sealed class CartItem : Entity
{
    public Guid CartId { get; private set; }
    public Guid SkuId { get; private set; }
    public int Quantity { get; private set; }

    private CartItem() { }

    public static CartItem Create(Guid cartId, Guid skuId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentInvalidException("Quantity must be greater than zero.");

        return new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            SkuId = skuId,
            Quantity = quantity
        };
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentInvalidException("Quantity must be greater than zero.");
        Quantity = quantity;
    }
}
