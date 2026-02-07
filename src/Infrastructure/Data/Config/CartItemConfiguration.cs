using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CartId)
            .IsRequired()
            .HasComment("購物車 ID");

        builder.Property(x => x.ProductId)
            .IsRequired()
            .HasComment("商品 ID");

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasComment("數量");

        builder.HasIndex(x => new { x.CartId, x.ProductId })
            .IsUnique()
            .HasDatabaseName("IX_CartItem_CartId_ProductId");

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
