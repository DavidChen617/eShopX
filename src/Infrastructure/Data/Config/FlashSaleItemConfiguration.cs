using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class FlashSaleItemConfiguration : IEntityTypeConfiguration<FlashSaleItem>
{
    public void Configure(EntityTypeBuilder<FlashSaleItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FlashSaleId)
            .IsRequired()
            .HasComment("所屬秒殺活動");

        builder.Property(x => x.SlotId)
            .HasComment("所屬場次（可選）");

        builder.Property(x => x.ProductId)
            .IsRequired()
            .HasComment("商品 ID");

        builder.Property(x => x.FlashPrice)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasComment("秒殺價");

        builder.Property(x => x.StockTotal)
            .IsRequired()
            .HasComment("秒殺總庫存");

        builder.Property(x => x.StockRemaining)
            .IsRequired()
            .HasComment("剩餘庫存");

        builder.Property(x => x.Badge)
            .HasMaxLength(20)
            .HasComment("標籤（Hot、限量、新品等）");

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("排序順序");

        builder.Property(x => x.PurchaseLimit)
            .IsRequired()
            .HasDefaultValue(1)
            .HasComment("每人限購數量");

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FlashSaleId, x.SlotId, x.SortOrder });
        builder.HasIndex(x => x.ProductId);
    }
}