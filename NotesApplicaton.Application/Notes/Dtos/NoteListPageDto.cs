namespace NotesApplication.Application.Notes.Dtos;

public sealed record NoteListPageDto
{
    public IReadOnlyList<NoteListItemDto> Items { get; init; } = [];
    public bool HasMore { get; set; }
    public int Page { get; init; }
    public int Size { get; init; }
}
