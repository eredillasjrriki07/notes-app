namespace NotesApplication.Application.Notes.Dtos;

public sealed record NoteDto
{
    public Guid Id { get; set; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public uint Version { get; init; }
}
