using Domain.Aggregates.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(v => v.Id);
        builder.ToTable(t => t.HasComment("商品款式（依顏色區分）"));

        builder.Property(v => v.ProductId).IsRequired().HasComment("所屬商品 ID");
        builder.Property(v => v.Color).IsRequired().HasMaxLength(50).HasComment("顏色名稱");

        builder.HasMany(v => v.Skus)
            .WithOne()
            .HasForeignKey(s => s.ProductVariantId);

        builder.HasMany(v => v.Images)
            .WithOne()
            .HasForeignKey(i => i.ProductVariantId);
    }
}
