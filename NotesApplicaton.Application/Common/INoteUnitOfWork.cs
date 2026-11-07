namespace NotesApplication.Application.Common;

public interface INoteUnitOfWork
{
    Task ExecuteAsync(Func<CancellationToken, Task> work, CancellationToken ct = default);

    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct = default);
}
