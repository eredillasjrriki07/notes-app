using NotesApplication.Domain.Notes;

namespace NotesApplication.Application.Notes;

public static class NoteMessages
{
    public const string NotFound = "That note is not on record.";
    public const string MissingVersion = "This request is missing the version of the product it is editing.";
    public const string BlankTitle = "Give the note a name.";
    public static readonly string TitleTooLong = $"Keep the note title to {NoteTitle.MaxLength} characters or fewer.";
    public static readonly string ContentTooLong = NoteRules.ContentTooLong;

}
