using Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.HasKey(e => e.Id);
        builder.ToTable(t => t.HasComment("Outbox 事件佇列（保證至少一次送出 Domain Event）"));

        builder.Property(e => e.EventType).IsRequired().HasMaxLength(200).HasComment("事件類型名稱（如 PaymentPaidEvent）");
        builder.Property(e => e.Payload).IsRequired().HasComment("事件序列化內容（JSON）");
        builder.Property(e => e.Status).HasComment("處理狀態（Pending、Processed、Failed）");
        builder.Property(e => e.RetryCount).HasComment("已重試次數（上限 3 次後標記為 Failed）");
        builder.Property(e => e.ProcessedAt).HasComment("事件成功處理的時間");

        builder.HasIndex(e => e.Status);
    }
}
