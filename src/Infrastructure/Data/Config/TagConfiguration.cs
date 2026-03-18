using Domain.Aggregates.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.Ignore(t => t.DomainEvents);

        builder.HasKey(t => t.Id);
        builder.ToTable(t => t.HasComment("商品標籤"));

        builder.Property(t => t.Name).IsRequired().HasMaxLength(100).HasComment("標籤名稱（唯一）");
        builder.HasIndex(t => t.Name).IsUnique();
        builder.Property(t => t.Type).HasComment("標籤類型（材質、風格、季節等）");
    }
}
