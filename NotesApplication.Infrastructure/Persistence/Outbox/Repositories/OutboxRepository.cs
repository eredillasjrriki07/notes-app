using NotesApplication.Application.Outbox;
using NotesApplication.Domain.Outbox;

namespace NotesApplication.Infrastructure.Persistence.Outbox.Repositories;

public sealed class OutboxRepository(NotesDbContext _db) : IOutboxRepository
{
    public void Add(OutboxMessage message)
        => _db.OutboxMessages.Add(message);
}
