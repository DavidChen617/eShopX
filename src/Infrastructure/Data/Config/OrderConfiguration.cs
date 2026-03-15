using eShopX.Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Ignore(o => o.DomainEvents);

        builder.HasKey(o => o.Id);
        builder.ToTable(t => t.HasComment("訂單主表"));

        builder.Property(o => o.UserId).IsRequired().HasComment("下單使用者 ID");
        builder.Property(o => o.Status).HasComment("訂單狀態（待付款、已付款、已出貨、已完成）");

        builder.OwnsOne(o => o.TotalAmount, b =>
            b.Property(m => m.Amount).HasColumnName("TotalAmount").HasComment("訂單總金額"));

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId);
    }
}
