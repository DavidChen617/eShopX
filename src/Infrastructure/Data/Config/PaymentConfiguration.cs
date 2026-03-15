using eShopX.Domain.Aggregates.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Ignore(p => p.DomainEvents);

        builder.HasKey(p => p.Id);
        builder.ToTable(t => t.HasComment("付款記錄"));

        builder.Property(p => p.OrderId).IsRequired().HasComment("關聯訂單 ID");
        builder.Property(p => p.Method).HasComment("付款方式（LinePay、PayPal、ECPay 等）");
        builder.Property(p => p.Status).HasComment("付款狀態（待付款、已付款、失敗）");
        builder.Property(p => p.TransactionId).HasComment("第三方金流交易序號");
        builder.Property(p => p.PaymentUrl).HasComment("付款頁面網址（由金流方產生）");
        builder.Property(p => p.PaidAt).HasComment("實際付款完成時間");

        builder.OwnsOne(p => p.Amount, b =>
            b.Property(m => m.Amount).HasColumnName("Amount").HasComment("付款金額"));
    }
}
