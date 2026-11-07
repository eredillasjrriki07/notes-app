using NotesApplication.Application.Common;

namespace NotesApplication.Infrastructure.Persistence;

public sealed class NoteUnitOfWork(NotesDbContext _db) : INoteUnitOfWork
{
    private bool _executing;
    public Task ExecuteAsync(Func<CancellationToken, Task> work, CancellationToken ct = default) =>
        ExecuteAsync<bool>(async token => { await work(token); return true; }, ct);

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct = default)
    {
        if (_executing)
            throw new InvalidOperationException("ExecuteAsync is not re-entrant");
        _executing = true;

        try
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);

            var result = await work(ct);

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return result;
        }
        finally 
        {
            _executing = false;
        }
    }
}
