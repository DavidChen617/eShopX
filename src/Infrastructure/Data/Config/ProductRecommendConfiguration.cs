using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class ProductRecommendConfiguration : IEntityTypeConfiguration<ProductRecommend>
{
    public void Configure(EntityTypeBuilder<ProductRecommend> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired()
            .HasComment("商品 ID");

        builder.Property(x => x.RecommendType)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("homepage")
            .HasComment("推薦類型（homepage、category、similar 等）");

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("排序順序");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("是否啟用");

        builder.Property(x => x.StartsAt)
            .HasComment("生效開始時間");

        builder.Property(x => x.EndsAt)
            .HasComment("生效結束時間");

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.RecommendType, x.IsActive, x.SortOrder });
        builder.HasIndex(x => x.ProductId);
    }
}