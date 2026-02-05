using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Refresh Token 值");

        builder.Property(x => x.ExpireAt)
            .IsRequired()
            .HasComment("過期時間");

        builder.Property(x => x.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("是否已撤銷");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Token)
            .IsUnique();
    }
}