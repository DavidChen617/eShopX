using Domain.Aggregates.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.ToTable(t => t.HasComment("購物車項目"));

        builder.Property(i => i.CartId).IsRequired().HasComment("所屬購物車 ID");
        builder.Property(i => i.SkuId).IsRequired().HasComment("商品 SKU ID");
        builder.Property(i => i.Quantity).HasComment("數量");
    }
}
