using Microsoft.EntityFrameworkCore;
using NotesApplication.Application.Notes;
using NotesApplication.Application.Notes.Dtos;
using NotesApplication.Infrastructure.Persistence.Configurations;

namespace NotesApplication.Infrastructure.Persistence.Queries;

internal sealed class NoteQueries(NotesDbContext _db) : INoteQueries
{

    public const string LikeEscape = "\\";

    public Task<NoteDto?> GetAsync(Guid id, CancellationToken ct = default) =>
        _db.Notes.AsNoTracking()
            .Where(n => n.Id == id && !n.IsArchived)
            .Select(n => new NoteDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                CreatedAt = EF.Property<DateTime>(n, AuditColumns.CreatedAt),
                UpdatedAt = EF.Property<DateTime>(n, AuditColumns.UpdatedAt),
                Version = EF.Property<uint>(n, RowVersion.Name),
            })
            .SingleOrDefaultAsync(ct);

    public async Task<NoteListPageDto> ListAsync(NoteListQuery query, CancellationToken ct = default)
    {
        var notes = _db.Notes.AsNoTracking().Where(n => !n.IsArchived);
        if (query.Text.Length > 0)
        {
            var pattern = "%" + EscapeLike(query.Text) + "%";
            notes = notes.Where(n => EF.Functions.Like(n.Title, pattern, LikeEscape));
        }

        var rows = await notes
            .OrderBy(n => n.Title)
            .ThenBy(n => n.Id)
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size + 1)
            .Select(n => new NoteListItemDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
            })
            .ToListAsync(ct);

        return new NoteListPageDto
        {
            Items = rows.Take(query.Size).ToList(),
            HasMore = rows.Count > query.Size,
            Page = query.Page,
            Size = query.Size,
        };
    }

    private static string EscapeLike(string text) =>
        text.Replace(LikeEscape, LikeEscape + LikeEscape)
            .Replace("%", LikeEscape + "%")
            .Replace("_", LikeEscape + "_");
}
