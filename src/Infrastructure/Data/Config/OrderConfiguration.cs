using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasComment("下單使用者 ID");

        builder.Property(x => x.Status)
            .IsRequired()
            .HasComment("訂單狀態");

        builder.Property(x => x.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasComment("訂單總金額");

        builder.Property(x => x.PaymentMethod)
            .IsRequired()
            .HasMaxLength(30)
            .HasComment("付款方式");

        builder.Property(x => x.PaidAt)
            .HasComment("付款時間");

        builder.Property(x => x.ShippingName)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("收件人姓名");

        builder.Property(x => x.ShippingAddress)
            .IsRequired()
            .HasMaxLength(300)
            .HasComment("收件地址");

        builder.Property(x => x.ShippingPhone)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("收件人電話");

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
