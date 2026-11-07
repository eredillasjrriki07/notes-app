using NotesApplication.Application.Notes.Dtos;
using NotesApplication.Application.Notes.RequestDtos;

namespace NotesApplication.Application.Notes;

public interface INoteService
{
    Task<NoteDto> CreateAsync(CreateNoteRequestDto request, CancellationToken ct = default);

    Task<NoteDto> UpdateAsync(Guid id, UpdateNoteRequestDto request, CancellationToken ct = default);
}
