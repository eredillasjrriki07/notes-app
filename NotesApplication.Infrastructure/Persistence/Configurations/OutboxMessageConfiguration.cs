using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotesApplication.Domain.Outbox;

namespace NotesApplication.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(m => m.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        builder.Property(m => m.Payload).HasColumnName("payload").IsRequired();
        builder.Property(m => m.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Attempts).HasColumnName("attempts");
        builder.Property(m => m.OccurredAtUtc).HasColumnName("occurred_at");
        builder.Property(m => m.NextAttemptAtUtc).HasColumnName("next_attempt_at");
        builder.Property(m => m.ProcessedAtUtc).HasColumnName("processed_at");
        builder.Property(m => m.LastError).HasColumnName("last_error").HasMaxLength(1000);

        builder.HasIndex(m => new { m.Status, m.NextAttemptAtUtc });   // the polling query
    }
}
