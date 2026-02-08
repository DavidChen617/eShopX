using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class ReviewImageConfiguration : IEntityTypeConfiguration<ReviewImage>
{
    public void Configure(EntityTypeBuilder<ReviewImage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("圖片 URL");

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("排序");

        builder.HasIndex(x => x.ReviewId);

        builder.HasOne(x => x.Review)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
