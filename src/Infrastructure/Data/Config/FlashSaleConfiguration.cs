using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class FlashSaleConfiguration : IEntityTypeConfiguration<FlashSale>
{
    public void Configure(EntityTypeBuilder<FlashSale> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("秒殺活動標題");

        builder.Property(x => x.Subtitle)
            .HasMaxLength(200)
            .HasComment("副標題說明");

        builder.Property(x => x.StartsAt)
            .IsRequired()
            .HasComment("活動開始時間");

        builder.Property(x => x.EndsAt)
            .IsRequired()
            .HasComment("活動結束時間");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("是否啟用");

        builder.HasMany(x => x.Slots)
            .WithOne(x => x.FlashSale)
            .HasForeignKey(x => x.FlashSaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.FlashSale)
            .HasForeignKey(x => x.FlashSaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.IsActive, x.StartsAt, x.EndsAt });
    }
}