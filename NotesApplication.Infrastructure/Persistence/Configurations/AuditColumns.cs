using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NotesApplication.Infrastructure.Persistence.Configurations;

internal static class AuditColumns
{
    public const string CreatedAt = "created_at";
    public const string UpdatedAt = "updated_at";

    public static void Apply<T>(EntityTypeBuilder<T> builder) where T : class
    {
         builder.Property<DateTime>(CreatedAt).HasColumnName(CreatedAt);
         builder.Property<DateTime>(UpdatedAt).HasColumnName(UpdatedAt);
    }
}

internal static class RowVersion
{
    public const string Name = "version";

    public static void Apply<T>(EntityTypeBuilder<T> builder) where T : class
        => builder.Property<uint>(Name).HasColumnName(Name).IsConcurrencyToken();
}