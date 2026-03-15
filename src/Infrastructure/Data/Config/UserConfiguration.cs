using eShopX.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Ignore(u => u.DomainEvents);

        builder.HasKey(u => u.Id);
        builder.ToTable(t => t.HasComment("使用者主表"));

        builder.Property(u => u.Name).IsRequired().HasMaxLength(100).HasComment("顯示名稱");
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200).HasComment("電子信箱（唯一）");
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Roles).HasComment("角色旗標（Flags enum，可複選）");

        builder.OwnsOne(u => u.Avatar, b =>
        {
            b.Property(a => a.Url).HasComment("頭像圖片網址");
            b.Property(a => a.PublicId).HasComment("Cloudinary Public ID");
        });

        builder.HasMany(u => u.AuthProviders)
            .WithOne()
            .HasForeignKey(p => p.UserId);
    }
}
