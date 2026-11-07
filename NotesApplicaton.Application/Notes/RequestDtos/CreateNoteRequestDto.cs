namespace NotesApplication.Application.Notes.RequestDtos;

public sealed class CreateNoteRequestDto
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;

}
