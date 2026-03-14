using eShopX.Domain.Exceptions;

namespace eShopX.Domain.Aggregates.Carts;

public sealed class Cart : AggregateRoot
{
    private readonly List<CartItem> _items = [];

    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<CartItem> Items => _items;

    private Cart() { }

    public static Cart Create(Guid userId)
    {
        return new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(Guid skuId, int quantity)
    {
        var existing = _items.FirstOrDefault(i => i.SkuId == skuId);
        if (existing is not null)
        {
            existing.UpdateQuantity(existing.Quantity + quantity);
            return;
        }

        _items.Add(CartItem.Create(Id, skuId, quantity));
    }

    public void RemoveItem(Guid skuId)
    {
        var item = _items.FirstOrDefault(i => i.SkuId == skuId)
            ?? throw new ArgumentInvalidException($"Item with SkuId '{skuId}' not found in cart.");
        _items.Remove(item);
    }

    public void UpdateQuantity(Guid skuId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.SkuId == skuId)
            ?? throw new ArgumentInvalidException($"Item with SkuId '{skuId}' not found in cart.");
        item.UpdateQuantity(quantity);
    }

    public void Clear() => _items.Clear();
}
