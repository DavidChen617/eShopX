namespace eShopX.Domain.Aggregates.Sizes;

[Flags]
public enum SizeType
{
    Clothing = 1,
    Pants    = 2,
    Hat      = 4,
    Shoes    = 8
}
