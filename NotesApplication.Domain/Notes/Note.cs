using NotesApplication.Domain.Common;
using NotesApplication.Domain.Notes.Events;

namespace NotesApplication.Domain.Notes;

public sealed class Note : Entity, IDomainEventCarrier
{
    public const int ContentLength = 500;

    private readonly List<object> _events = [];

    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public bool IsArchived { get; set; }

    public IReadOnlyList<object> DomainEvents => _events;

    public Note() { }

    public Note(string title, string content)
    {
        Id = Guid.NewGuid();
        Title = title;
        Content = content;
    }

    public static Note Create(NoteTitle title, string content)
    {
        var note = new Note
        {
            Id = Guid.CreateVersion7(),
        };
        note.SetDetails(title, Clean(content));
        note._events.Add(new NoteCreatedEvent(note.Id, note.Title, note.Content));
        return note;
    }

    private void SetDetails(NoteTitle title, string content)
    {
        Rule.Require(content is null || content.Length <= ContentLength, NoteRules.ContentTooLong);
        Title = title.Value;
        Content = content;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public void ClearDomainEvents() => _events.Clear();
}
public static class NoteRules 
{
    public static readonly string ContentTooLong = $"Content must be {Note.ContentLength} characters or fewer.";
}