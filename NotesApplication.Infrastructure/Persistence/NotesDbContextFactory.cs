using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotesApplication.Infrastructure.Persistence;

public class NotesDbContextFactory : IDesignTimeDbContextFactory<NotesDbContext>
{
    public NotesDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Host=localhost;Port=5432;Database=notesdb;Username=postgres;Password=admin";

        var options = new DbContextOptionsBuilder<NotesDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new NotesDbContext(options);
    }
}
