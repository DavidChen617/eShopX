using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Rating)
            .IsRequired()
            .HasComment("評分 1~5");

        builder.Property(x => x.Content)
            .HasMaxLength(1000)
            .HasComment("評價內容");

        builder.Property(x => x.IsAnonymous)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("是否匿名");

        // 每個 OrderItem 只能評價一次
        builder.HasIndex(x => x.OrderItemId)
            .IsUnique();

        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.UserId);

        // 關聯
        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.OrderItem)
            .WithMany()
            .HasForeignKey(x => x.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
