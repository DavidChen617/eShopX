using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId)
            .IsRequired()
            .HasComment("所屬訂單 ID");

        builder.Property(x => x.ProductId)
            .IsRequired()
            .HasComment("商品 ID（記錄用）");

        builder.Property(x => x.ProductName)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("商品名稱（快照）");

        builder.Property(x => x.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasComment("商品單價（快照）");

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasComment("購買數量");
    }
}
