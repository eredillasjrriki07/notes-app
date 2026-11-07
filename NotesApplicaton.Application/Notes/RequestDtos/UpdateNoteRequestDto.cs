namespace NotesApplication.Application.Notes.RequestDtos;

public sealed class UpdateNoteRequestDto
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public uint Version { get; set; }
}
