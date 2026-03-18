using Domain.Aggregates.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(i => i.Id);
        builder.ToTable(t => t.HasComment("商品圖片"));

        builder.Property(i => i.ProductVariantId).IsRequired().HasComment("所屬款式 ID");
        builder.Property(i => i.Url).IsRequired().HasComment("圖片網址");
        builder.Property(i => i.PublicId).IsRequired().HasComment("Cloudinary Public ID，用於更新或刪除");
        builder.Property(i => i.IsPrimary).HasComment("是否為主圖");
        builder.Property(i => i.SortOrder).HasComment("排序順序（由小到大）");
    }
}
