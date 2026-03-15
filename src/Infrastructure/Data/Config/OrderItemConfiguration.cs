using eShopX.Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.ToTable(t => t.HasComment("訂單明細"));

        builder.Property(i => i.OrderId).IsRequired().HasComment("所屬訂單 ID");
        builder.Property(i => i.SkuId).IsRequired().HasComment("下單時的 SKU ID");
        builder.Property(i => i.Quantity).HasComment("購買數量");

        builder.OwnsOne(i => i.UnitPrice, b =>
            b.Property(m => m.Amount).HasColumnName("UnitPrice").HasComment("下單當下的單價快照"));

        builder.OwnsOne(i => i.TotalPrice, b =>
            b.Property(m => m.Amount).HasColumnName("TotalPrice").HasComment("該明細小計（單價 × 數量）"));

        builder.OwnsOne(i => i.Snapshot, b =>
        {
            b.Property(s => s.ProductName).HasColumnName("ProductName").HasComment("下單當下的商品名稱快照");
            b.Property(s => s.Color).HasColumnName("Color").HasComment("下單當下的顏色快照");
            b.Property(s => s.Size).HasColumnName("Size").HasComment("下單當下的尺寸快照");
        });
    }
}
