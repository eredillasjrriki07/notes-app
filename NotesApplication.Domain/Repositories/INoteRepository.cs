using NotesApplication.Domain.Notes;

namespace NotesApplication.Domain.Repositories;

public interface INoteRepository
{
    Task<Note?> GetAsync(Guid id, CancellationToken ct = default);

    void Add(Note note);

    void ExpectVersion(Note note, uint version);
}
