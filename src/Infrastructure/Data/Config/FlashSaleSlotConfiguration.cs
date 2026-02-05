using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class FlashSaleSlotConfiguration : IEntityTypeConfiguration<FlashSaleSlot>
{
    public void Configure(EntityTypeBuilder<FlashSaleSlot> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FlashSaleId)
            .IsRequired()
            .HasComment("所屬秒殺活動");

        builder.Property(x => x.Label)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("場次標籤（如 10:00）");

        builder.Property(x => x.StartsAt)
            .IsRequired()
            .HasComment("場次開始時間");

        builder.Property(x => x.EndsAt)
            .IsRequired()
            .HasComment("場次結束時間");

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("排序順序");

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Slot)
            .HasForeignKey(x => x.SlotId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.FlashSaleId, x.SortOrder });
    }
}