namespace NotesApplicaton.Application.Common;

public interface IDomainEventRecorder
{
    void Record(string eventType, string entityType, string entityId, object payload);
}
