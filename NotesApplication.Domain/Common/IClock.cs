namespace NotesApplication.Domain.Common;

public interface IClock
{
    DateTime UtcNow { get; }
}
