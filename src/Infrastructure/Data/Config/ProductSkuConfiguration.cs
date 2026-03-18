using Domain.Aggregates.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class ProductSkuConfiguration : IEntityTypeConfiguration<ProductSku>
{
    public void Configure(EntityTypeBuilder<ProductSku> builder)
    {
        builder.HasKey(s => s.Id);
        builder.ToTable(t => t.HasComment("商品 SKU（款式 × 尺寸的最小庫存單位）"));

        builder.Property(s => s.ProductVariantId).IsRequired().HasComment("所屬款式 ID");
        builder.Property(s => s.SizeId).HasComment("所屬尺寸 ID（無尺寸商品為 null）");
        builder.Property(s => s.StockQuantity).HasComment("目前庫存數量");

        builder.OwnsOne(s => s.Price, b =>
            b.Property(m => m.Amount).HasColumnName("Price").HasComment("售價"));

        builder.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
    }
}
