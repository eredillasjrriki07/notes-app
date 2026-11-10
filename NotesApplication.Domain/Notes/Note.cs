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

    public void Update(NoteTitle title, string content)
    {
        EnsureNotArchived();
        var cleaned = Clean(content);

        var changes = new List<NoteFieldChanged>(2);
        Track(changes, "title", Title, title.Value);
        Track(changes, "content", Content, content);
        if (changes.Count == 0)
            return;

        SetDetails(title, cleaned);
        _events.Add(new NoteUpdatedEvent(Id, Title, changes));
    }

    private void SetDetails(NoteTitle title, string content)
    {
        Rule.Require(content is null || content.Length <= ContentLength, NoteRules.ContentTooLong);
        Title = title.Value;
        Content = content;
    }

    private void EnsureNotArchived() => Rule.Require(!IsArchived, NoteRules.Archived);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public void ClearDomainEvents() => _events.Clear();

    private static void Track(List<NoteFieldChanged> changes, string field, string? from, string? to)
    {
        if (!string.Equals(from, to, StringComparison.Ordinal))
            changes.Add(new NoteFieldChanged(field, from, to));
    }
}
public static class NoteRules 
{
    public const string Archived = "This note was archived.";
    public static readonly string ContentTooLong = $"Content must be {Note.ContentLength} characters or fewer.";
}