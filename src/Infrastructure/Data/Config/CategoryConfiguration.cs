using Domain.Aggregates.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Ignore(c => c.DomainEvents);

        builder.HasKey(c => c.Id);
        builder.ToTable(t => t.HasComment("商品分類"));

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100).HasComment("分類名稱（唯一）");
        builder.HasIndex(c => c.Name).IsUnique();
    }
}
