using Microsoft.EntityFrameworkCore;
using NotesApplication.Application.Common.Exceptions;
using NotesApplication.Application.Outbox;
using NotesApplication.Application.Outbox.Dto;
using NotesApplication.Domain.Outbox.Enums;

namespace NotesApplication.Infrastructure.Persistence.Outbox.Repositories;

public sealed class OutboxAdminRepository(NotesDbContext _db) : IOutboxAdminRepository
{
    public async Task<IReadOnlyList<OutboxMessageDto>> ListFailedAsync(CancellationToken ct = default)
        => await _db.OutboxMessages.AsNoTracking()
            .Where(m => m.Status == OutboxStatus.Failed)
            .OrderBy(m => m.OccurredAtUtc)
            .Select(m => new OutboxMessageDto
            {
                Id = m.Id,
                Type = m.Type,
                Attempts = m.Attempts,
                LastError = m.LastError,
                OccurredAtUtc = m.OccurredAtUtc,
            })
            .ToListAsync(ct);

    public async Task RetryAsync(Guid id, CancellationToken ct = default)
    {
        var message = await _db.OutboxMessages.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException("Outbox message not found.");

        message.Requeue(DateTime.UtcNow);
        await _db.SaveChangesAsync(ct);
    }
}
