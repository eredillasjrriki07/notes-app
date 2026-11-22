namespace NotesApplication.Infrastructure.Persistence.Outbox.Options;

public class OutboxOptions
{
    public int PollIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 20;
    public int MaxAttempts { get; set; } = 10;
}
