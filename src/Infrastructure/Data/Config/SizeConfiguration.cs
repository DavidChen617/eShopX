using eShopX.Domain.Aggregates.Sizes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class SizeConfiguration : IEntityTypeConfiguration<Size>
{
    public void Configure(EntityTypeBuilder<Size> builder)
    {
        builder.Ignore(s => s.DomainEvents);

        builder.HasKey(s => s.Id);
        builder.ToTable(t => t.HasComment("尺寸選項"));

        builder.Property(s => s.Name).IsRequired().HasMaxLength(50).HasComment("尺寸名稱（S、M、L、XL 等）");
        builder.Property(s => s.Type).HasComment("尺寸分類（衣服、鞋子等）");
    }
}
