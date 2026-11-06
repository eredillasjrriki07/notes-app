namespace NotesApplication.Domain.Common;

public interface IDomainEventCarrier
{
    IReadOnlyList<object> DomainEvents { get; }

    void ClearDomainEvents();
}
