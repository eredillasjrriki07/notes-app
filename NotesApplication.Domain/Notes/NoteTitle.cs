using NotesApplication.Domain.Common;

namespace NotesApplication.Domain.Notes;

public sealed class NoteTitle : ValueObject
{
    public const int MaxLength = 200;
    public string Value { get; }

    public NoteTitle(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        Rule.Require(trimmed.Length > 0, "Title is required.");
        Rule.Require(trimmed.Length <= MaxLength, $"Title must be {MaxLength} characters or fewer.");
        Value = trimmed;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
