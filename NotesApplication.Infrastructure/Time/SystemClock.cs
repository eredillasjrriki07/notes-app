using NotesApplication.Domain.Common;

namespace NotesApplication.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
