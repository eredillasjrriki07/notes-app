namespace NotesApplication.Domain.Notes.Events;

public sealed record NoteCreatedEvent(Guid NoteId, string Title, string Content);
