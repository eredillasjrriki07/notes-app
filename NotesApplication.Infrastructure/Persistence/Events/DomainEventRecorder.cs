using NotesApplication.Domain.Common;
using NotesApplication.Application.Common;
using System.Text.Json;

namespace NotesApplication.Infrastructure.Persistence.Events;

internal sealed class DomainEventRecorder(NotesDbContext _db, IClock _clock) : IDomainEventRecorder
{
    public void Record(string eventType, string entityType, string entityId, object payload)
    {
        _db.DomainEvents.Add(new DomainEventRow
        {
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId,
            Payload = JsonSerializer.Serialize(payload, payload.GetType()),
            OccuredAt= _clock.UtcNow,
        });
    }
}
