using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("商品名稱");

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .HasComment("商品描述");

        builder.Property(x => x.Price)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasComment("商品單價");

        builder.Property(x => x.StockQuantity)
            .IsRequired()
            .HasComment("庫存數量");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("是否上架");

        builder.Property(x => x.CategoryId)
            .HasComment("分類 ID");

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.CategoryId);

        // 賣家關聯
        builder.Property(x => x.SellerId)
            .HasComment("賣家 ID");

        builder.HasOne(x => x.Seller)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SellerId);
    }
}