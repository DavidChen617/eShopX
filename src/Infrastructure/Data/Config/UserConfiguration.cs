using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("使用者名稱");

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(2048)
            .HasComment("頭像 URL");

        builder.Property(x => x.AvatarPublicId)
            .HasMaxLength(255)
            .HasComment("頭像 PublicId");

        builder.Property(x => x.AvatarFormat)
            .HasMaxLength(20)
            .HasComment("頭像格式");

        builder.Property(x => x.AvatarWidth)
            .HasComment("頭像寬度");

        builder.Property(x => x.AvatarHeight)
            .HasComment("頭像高度");

        builder.Property(x => x.AvatarBytes)
            .HasComment("頭像大小(Bytes)");

        builder.Property(x => x.Email)
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(200)
            .HasComment("電子信箱");

        builder.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("聯絡電話");

        builder.Property(x => x.Address)
            .HasMaxLength(300)
            .HasComment("聯絡地址");

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("密碼雜湊值");

        builder.HasIndex(x => x.Email)
            .IsUnique();

        // 角色相關
        builder.Property(x => x.IsAdmin)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("是否為管理員");

        builder.Property(x => x.IsSeller)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("是否為賣家");

        // 賣家申請相關
        builder.Property(x => x.SellerStatus)
            .HasComment("賣家申請狀態：0=申請中, 1=已通過, 2=已拒絕");

        builder.Property(x => x.SellerAppliedAt)
            .HasComment("賣家申請時間");

        builder.Property(x => x.SellerApprovedAt)
            .HasComment("賣家審核通過時間");

        builder.Property(x => x.SellerApprovedBy)
            .HasComment("審核的管理員 ID");

        builder.Property(x => x.SellerRejectionReason)
            .HasMaxLength(500)
            .HasComment("拒絕原因");
    }
}
