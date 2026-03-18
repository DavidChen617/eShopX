using Domain.Aggregates.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.HasKey(t => new { t.ProductId, t.TagId });
        builder.ToTable(t => t.HasComment("商品與標籤的對應關係"));

        builder.Property(t => t.ProductId).HasComment("商品 ID");
        builder.Property(t => t.TagId).HasComment("標籤 ID");
    }
}
