namespace NotesApplication.Application.Outbox.Dto;

public sealed class OutboxMessageDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public int Attempts { get; init; }
    public string? LastError { get; init; }
    public DateTime OccurredAtUtc { get; init; }
}
