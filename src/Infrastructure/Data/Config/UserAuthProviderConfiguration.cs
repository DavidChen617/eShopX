using Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class UserAuthProviderConfiguration : IEntityTypeConfiguration<UserAuthProvider>
{
    public void Configure(EntityTypeBuilder<UserAuthProvider> builder)
    {
        builder.HasKey(p => p.Id);
        builder.ToTable(t => t.HasComment("使用者登入方式（本地密碼 / 第三方 OAuth）"));

        builder.Property(p => p.UserId).IsRequired().HasComment("所屬使用者 ID");
        builder.Property(p => p.Provider).IsRequired().HasComment("登入提供者（Local、Google、Line）");
        builder.Property(p => p.ProviderUserId).HasComment("第三方平台的使用者識別碼（sub）");
        builder.Property(p => p.PasswordHash).HasComment("本地登入的雜湊密碼（第三方登入為 null）");
    }
}
