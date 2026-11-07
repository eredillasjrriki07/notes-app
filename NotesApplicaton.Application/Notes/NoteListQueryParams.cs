namespace NotesApplication.Application.Notes;

public sealed record NoteListQuery(string Text, int Page, int Size);

public sealed record NoteListNormalization(NoteListQuery? Query, string? Error);

public sealed record NoteListQueryParams(string? Q, string? Page, string? Size)
{
    public const int DefaultSize = 20;
    public const int MaxSize = 100;
    public const int MaxPage = 10_000;
    public const int MaxTextLength = 100;

    public NoteListNormalization Normalize()
    {
        var page = 1;
        if (!string.IsNullOrWhiteSpace(Page) && (!int.TryParse(Page.Trim(), out page) || page is < 1 or > MaxPage))
            return Fail($"page must be a whole number from 1 to {MaxPage}, not '{Page}'.");

        var size = DefaultSize;
        if (!string.IsNullOrWhiteSpace(Size) && (!int.TryParse(Size.Trim(), out size) || size is < 1 or > MaxSize))
            return Fail($"size must be a whole number from 1 to {MaxSize}, not '{Size}'.");

        var text = Q?.Trim() ?? string.Empty;
        if (text.Length > MaxTextLength)
            return Fail($"q must be {MaxTextLength} characters or fewer.");

        return new(new NoteListQuery(text, page, size), null);
    }

    private static NoteListNormalization Fail(string error) => new(null, error);
}
