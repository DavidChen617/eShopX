using eShopX.Domain.Aggregates.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.Ignore(c => c.DomainEvents);

        builder.HasKey(c => c.Id);
        builder.ToTable(t => t.HasComment("購物車（每位使用者一個）"));

        builder.Property(c => c.UserId).IsRequired().HasComment("所屬使用者 ID");

        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(i => i.CartId);
    }
}
