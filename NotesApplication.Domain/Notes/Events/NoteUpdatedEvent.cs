namespace NotesApplication.Domain.Notes.Events;

public sealed record NoteUpdatedEvent(Guid NoteId, string Title, IReadOnlyList<NoteFieldChanged> Changes);

public sealed record NoteFieldChanged(string Field, string? From, string? To);
