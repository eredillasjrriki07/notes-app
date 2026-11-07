namespace NotesApplication.Application.Notes.Dtos;

public sealed record NoteListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}
