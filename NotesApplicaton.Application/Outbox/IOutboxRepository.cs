using NotesApplication.Domain.Outbox;

namespace NotesApplication.Application.Outbox;

public interface IOutboxRepository
{
    void Add(OutboxMessage message);
}
