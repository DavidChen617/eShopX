using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Banner 標題");

        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("圖片網址");

        builder.Property(x => x.Link)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("點擊連結");

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

        builder.HasIndex(x => new { x.IsActive, x.SortOrder });
        
        builder.Property(x => x.ImagePublicId)
            .HasMaxLength(200)
            .HasComment("圖片 PublicId");

        builder.Property(x => x.ImageFormat)
            .HasMaxLength(20)
            .HasComment("圖片格式");

        builder.Property(x => x.ImageWidth)
            .HasComment("圖片寬度");

        builder.Property(x => x.ImageHeight)
            .HasComment("圖片高度");

        builder.Property(x => x.ImageBytes)
            .HasComment("圖片大小(Byte)");
    }
}
