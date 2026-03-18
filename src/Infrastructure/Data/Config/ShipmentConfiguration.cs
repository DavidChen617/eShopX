using Domain.Aggregates.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.Ignore(s => s.DomainEvents);

        builder.HasKey(s => s.Id);
        builder.ToTable(t => t.HasComment("物流出貨記錄（TPH：CVS 超取 / Home 宅配）"));

        builder.Property(s => s.OrderId).IsRequired().HasComment("關聯訂單 ID");
        builder.Property(s => s.LogisticsId).HasComment("綠界物流訂單編號");
        builder.Property(s => s.LogisticsSubType).HasComment("物流子類型（FAMIC2C、UNIMARTC2C、TCAT 等）");
        builder.Property(s => s.LogisticsStatus).HasComment("物流狀態代碼（來自綠界回調）");
        builder.Property(s => s.LogisticsStatusName).HasComment("物流狀態說明文字");
        builder.Property(s => s.UpdateStatusDate).HasComment("最後物流狀態更新時間");

        builder.HasDiscriminator<string>("ShipmentType")
            .HasValue<CVSShipment>("CVS")
            .HasValue<HomeShipment>("Home");
    }
}

public class CVSShipmentConfiguration : IEntityTypeConfiguration<CVSShipment>
{
    public void Configure(EntityTypeBuilder<CVSShipment> builder)
    {
        builder.Property(s => s.StoreId).HasComment("超商門市代號");
        builder.Property(s => s.StoreName).HasComment("超商門市名稱");
        builder.Property(s => s.CVSPaymentNo).HasComment("超商繳費代碼（貨到付款時使用）");

        builder.OwnsOne(s => s.Receiver, b =>
        {
            b.Property(r => r.Name).HasColumnName("ReceiverName").HasComment("收件人姓名");
            b.Property(r => r.CellPhone).HasColumnName("ReceiverPhone").HasComment("收件人手機號碼");
        });
    }
}

public class HomeShipmentConfiguration : IEntityTypeConfiguration<HomeShipment>
{
    public void Configure(EntityTypeBuilder<HomeShipment> builder)
    {
        builder.Property(s => s.ZipCode).HasComment("收件地址郵遞區號");
        builder.Property(s => s.Address).HasComment("收件詳細地址");
        builder.Property(s => s.BookingNote).HasComment("宅配備註（預約時段等）");

        builder.OwnsOne(s => s.Receiver, b =>
        {
            b.Property(r => r.Name).HasColumnName("ReceiverName").HasComment("收件人姓名");
            b.Property(r => r.CellPhone).HasColumnName("ReceiverPhone").HasComment("收件人手機號碼");
        });
    }
}
