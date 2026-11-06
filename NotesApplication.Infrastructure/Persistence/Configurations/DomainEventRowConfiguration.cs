using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotesApplication.Infrastructure.Persistence.Events;

namespace NotesApplication.Infrastructure.Persistence.Configurations;

internal sealed class DomainEventRowConfiguration : IEntityTypeConfiguration<DomainEventRow>
{
    public void Configure(EntityTypeBuilder<DomainEventRow> builder)
    {
        builder.ToTable("domain_events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
        builder.Property(e => e.EntityType).HasColumnName("entity_type").HasMaxLength(50).IsRequired();
        builder.Property(e => e.EntityId).HasColumnName("entity_id").HasMaxLength(36).IsRequired();
        builder.Property(e => e.Payload).HasColumnName("payload").IsRequired();
        builder.Property(e => e.OccuredAt).HasColumnName("occured_at");

        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.Id }, "ix_domain_events_entity");
    }
}
