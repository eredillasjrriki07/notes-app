using NotesApplication.Domain.Notes.Events;

namespace NotesApplication.Application.Events;

public static class NoteEvents
{
    public static (string EventType, object Payload) For(object domainEvent) => domainEvent switch
    {
        NoteCreatedEvent e => (NoteEventTypes.NoteCreated, NoteCreatedPayload(e)),
        NoteUpdatedEvent e => (NoteEventTypes.NoteUpdated, NoteUpdatedPayload(e)),
        _ => throw new InvalidOperationException($"{domainEvent.GetType().Name} has no note event mapping; add one before raising it.")
    };

    public static string EntityId(Guid id) => id.ToString("D");

    public static object NoteCreatedPayload(NoteCreatedEvent e) => new
    {
        label = e.Title,
        summary = $"added note “{e.Title}” with content {e.Content}",
    };

    public static object NoteUpdatedPayload(NoteUpdatedEvent e) => new
    {
        label = e.Title,
        summary = e.Changes is [{ Field: "title" } rename]
            ? $"changed title “{rename.From}” to “{rename.To}”"
            : $"updated note “{e.Title}”",
        changes = e.Changes.Select(c => new { field = c.Field, from = c.From, to = c.To}).ToList(),
    };
}

public static class NoteEventTypes
{
    public const string NoteEntityType = "Note";
    public const string NoteCreated = "notesapplication.note.created";
    public const string NoteUpdated = "notesapplication.note.updated";
}
