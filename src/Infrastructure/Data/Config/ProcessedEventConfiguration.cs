using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config;

public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("ProcessedEvents");

        builder.Property(x => x.Source)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EventId)
            .IsRequired();

        builder.HasIndex(x => new { x.Source, x.EventId })
            .IsUnique();
    }
}
