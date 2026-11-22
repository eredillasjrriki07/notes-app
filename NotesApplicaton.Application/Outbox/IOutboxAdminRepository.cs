using NotesApplication.Application.Outbox.Dto;

namespace NotesApplication.Application.Outbox;

public interface IOutboxAdminRepository
{
    Task<IReadOnlyList<OutboxMessageDto>> ListFailedAsync(CancellationToken ct = default);
    Task RetryAsync(Guid id, CancellationToken ct);
}
