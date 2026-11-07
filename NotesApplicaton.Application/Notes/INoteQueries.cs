using NotesApplication.Application.Notes.Dtos;

namespace NotesApplication.Application.Notes;

public interface INoteQueries
{
    Task<NoteListPageDto> ListAsync(NoteListQuery query, CancellationToken ct = default);

    Task<NoteDto?> GetAsync(Guid id, CancellationToken ct = default);
}
