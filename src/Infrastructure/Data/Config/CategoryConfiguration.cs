using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("分類名稱");

        builder.Property(x => x.Icon)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("圖標（emoji 或 icon name）");

        builder.Property(x => x.Link)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("點擊連結");

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("排序順序");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("是否啟用");

        builder.Property(x => x.ParentId)
            .HasComment("父分類 ID（支援子分類）");

        builder.HasIndex(x => new { x.IsActive, x.SortOrder });
        builder.HasIndex(x => x.ParentId);
    }
}
