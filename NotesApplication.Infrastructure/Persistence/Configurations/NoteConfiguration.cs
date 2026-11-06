using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotesApplication.Domain.Notes;

namespace NotesApplication.Infrastructure.Persistence.Configurations;

internal sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("notes");
        builder.HasKey(n => n.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(p => p.Title).HasColumnName("title").HasMaxLength(NoteTitle.MaxLength).IsRequired();
        builder.Property(p => p.Content).HasColumnName("content").HasMaxLength(Note.ContentLength).IsRequired();
        builder.Property(p => p.IsArchived).HasColumnName("archived");

        builder.Ignore(p => p.DomainEvents);

        builder.HasIndex(p => p.Title, "ix_notes_title");

        AuditColumns.Apply(builder);
        RowVersion.Apply(builder);
    }
}
