using Microsoft.EntityFrameworkCore;
using NotesApplication.Domain.Notes;
using NotesApplication.Infrastructure.Persistence.Configurations;
using NotesApplication.Infrastructure.Persistence.Events;

namespace NotesApplication.Infrastructure.Persistence;

public class NotesDbContext(DbContextOptions<NotesDbContext> options) : DbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<DomainEventRow> DomainEvents => Set<DomainEventRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotesDbContext).Assembly);

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampRoots();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        StampRoots();
        return base.SaveChanges();
    }

    private void StampRoots()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Metadata.FindProperty(AuditColumns.CreatedAt) is null)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(AuditColumns.CreatedAt).CurrentValue = now;
                    entry.Property(RowVersion.Name).CurrentValue = 1u;
                    break;

                case EntityState.Modified:
                    entry.Property(AuditColumns.UpdatedAt).CurrentValue = now;
                    entry.Property(AuditColumns.CreatedAt).IsModified = false;

                    var version = entry.Property(RowVersion.Name);
                    version.CurrentValue = (uint)version.OriginalValue + 1;
                    break;
            }
        }
    }
}
