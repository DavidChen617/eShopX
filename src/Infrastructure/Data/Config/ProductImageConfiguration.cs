using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .HasComment("商品 ID");

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(2048)
            .HasComment("圖片 URL");

        builder.Property(x => x.PublicId)
            .IsRequired()
            .HasMaxLength(255)
            .HasComment("圖片 PublicId");

        builder.Property(x => x.Format)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("圖片格式");

        builder.Property(x => x.Width)
            .HasComment("圖片寬度");

        builder.Property(x => x.Height)
            .HasComment("圖片高度");

        builder.Property(x => x.Bytes)
            .HasComment("圖片大小(Bytes)");

        builder.Property(x => x.IsPrimary)
            .HasDefaultValue(false)
            .HasComment("是否為封面圖");

        builder.Property(x => x.SortOrder)
            .HasDefaultValue(0)
            .HasComment("排序");

        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => new { x.ProductId, x.IsPrimary });

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId);
    }
}