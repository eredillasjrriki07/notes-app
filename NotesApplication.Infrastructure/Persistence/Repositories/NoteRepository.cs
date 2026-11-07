using Microsoft.EntityFrameworkCore;
using NotesApplication.Domain.Notes;
using NotesApplication.Domain.Repositories;
using NotesApplication.Infrastructure.Persistence.Configurations;

namespace NotesApplication.Infrastructure.Persistence.Repositories;

internal sealed class NoteRepository(NotesDbContext _db) : INoteRepository
{
    public Task<Note?> GetAsync(Guid id, CancellationToken ct = default) =>
        _db.Notes.SingleOrDefaultAsync(n => n.Id == id, ct);

    public void Add(Note note) => _db.Notes.Add(note);

    public void ExpectVersion(Note note, uint version)
    {
        var loaded = _db.Entry(note).Property<uint>(RowVersion.Name).OriginalValue;
        if (loaded != version)
            throw new DbUpdateConcurrencyException(
                $"Note {note.Id} is at version {loaded}; the request was made against version {version}");
    }
}
