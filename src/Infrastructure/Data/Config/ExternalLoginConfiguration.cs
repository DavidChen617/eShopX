using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasComment("使用者 Id");

        builder.Property(x => x.LoginProvider)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("外部登入提供者");

        builder.Property(x => x.ProviderUserId)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("外部使用者唯一識別(sub)");

        builder.Property(x => x.EmailAtLinkTime)
            .HasMaxLength(200)
            .HasComment("綁定時的 Email");

        builder.Property(x => x.LastLoginAt)
            .HasComment("最後登入時間");

        builder.HasIndex(x => new { x.LoginProvider, x.ProviderUserId })
            .IsUnique();

        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
            .WithMany(x => x.ExternalLogins)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
