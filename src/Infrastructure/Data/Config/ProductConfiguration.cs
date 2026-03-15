using eShopX.Domain.Aggregates.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Ignore(p => p.DomainEvents);

        builder.HasKey(p => p.Id);
        builder.ToTable(t => t.HasComment("商品主表"));

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200).HasComment("商品名稱");
        builder.Property(p => p.Description).HasComment("商品描述");
        builder.Property(p => p.Audience).HasComment("適用客群（男、女、中性）");
        builder.Property(p => p.IsActive).HasComment("是否上架販售");
        builder.Property(p => p.CategoryId).IsRequired().HasComment("所屬分類 ID");

        builder.HasMany(p => p.Variants)
            .WithOne()
            .HasForeignKey(v => v.ProductId);

        builder.HasMany(p => p.Tags)
            .WithOne()
            .HasForeignKey(t => t.ProductId);
    }
}
