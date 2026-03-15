using eShopX.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(t => t.Id);
        builder.ToTable(t => t.HasComment("使用者 Refresh Token"));

        builder.Property(t => t.UserId).IsRequired().HasComment("所屬使用者 ID");
        builder.Property(t => t.Token).IsRequired().HasMaxLength(500).HasComment("Token 值（唯一）");
        builder.HasIndex(t => t.Token).IsUnique();
        builder.Property(t => t.ExpireAt).HasComment("Token 到期時間");
        builder.Property(t => t.IsRevoked).HasComment("是否已被撤銷");
    }
}
