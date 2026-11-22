using NotesApplication.Domain.Common;
using NotesApplication.Domain.Outbox.Enums;

namespace NotesApplication.Domain.Outbox;

public sealed class OutboxMessage : Entity
{
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public OutboxStatus Status { get; private set; }
    public int Attempts { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public DateTime NextAttemptAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage() { }

    public OutboxMessage(string type, string payload, DateTime nowUtc)
    {
        Id = Guid.CreateVersion7();
        Type = type;
        Payload = payload;
        Status = OutboxStatus.Pending;
        OccurredAtUtc = nowUtc;
        NextAttemptAtUtc = nowUtc;
    }

    public static OutboxMessage Create(string type, string payload, DateTime nowUtc) =>
        new(type, payload, nowUtc);

    public void MarkDone(DateTime nowUtc)
    {
        Status = OutboxStatus.Done;
        ProcessedAtUtc = nowUtc;
        LastError = null;

    }
    public void MarkFailed(string error, DateTime nowUtc, int maxAttempts)
    {
        Attempts++;
        LastError = error.Length > 1000 ? error[..1000] : error;

        if (Attempts >= maxAttempts)
        {
            Status = OutboxStatus.Failed;
            return;
        }

        // backoff: 5s, 10s, 20s, 40s, then 60s max
        var delaySeconds = Math.Min(5 * Math.Pow(2, Attempts - 1), 60);
        NextAttemptAtUtc = nowUtc.AddSeconds(delaySeconds);
    }

    public void Requeue(DateTime nowUtc)
    {
        if (Status != OutboxStatus.Failed)
            throw new DomainRuleException("Only failed messages can be retried.");

        Status = OutboxStatus.Pending;
        Attempts = 0;
        NextAttemptAtUtc = nowUtc;
    }
}
