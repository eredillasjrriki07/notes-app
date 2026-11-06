namespace NotesApplication.Infrastructure.Persistence.Events;

public sealed class DomainEventRow
{
    public long Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccuredAt { get; set; }
}
