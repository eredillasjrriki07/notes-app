using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotesApplication.Application.Outbox;
using NotesApplication.Infrastructure.Persistence.Outbox.Options;
using NotesApplication.Domain.Outbox.Enums;
using Microsoft.EntityFrameworkCore;
using NotesApplication.Domain.Outbox;
using NotesApplication.Application.Events;
using System.Text.Json;

namespace NotesApplication.Infrastructure.Persistence.Outbox;

public sealed class OutboxProcessor(
    NotesDbContext _db,
    IEmailSender _emailSender,
    IOptions<OutboxOptions> _outboxOptions,
    IOptions<EmailOptions> _emailOptions,
    ILogger<OutboxProcessor> _logger
)
{
    public async Task ProcessDueAsync(CancellationToken ct)
    {
        var opts = _outboxOptions.Value;
        var now = DateTime.UtcNow;

        var due = await _db.OutboxMessages
            .Where(om => om.Status == OutboxStatus.Pending && om.NextAttemptAtUtc <= now)
            .OrderBy(om => om.NextAttemptAtUtc)
            .Take(opts.BatchSize)
            .ToListAsync(ct);

        foreach (var message in due)
        {
            try
            {
                await DispatchAsync(message, ct);
                message.MarkDone(now);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Outbox message {Id} failed (attempt {Attempt})", message.Id, message.Attempts + 1);
                message.MarkFailed(ex.Message, DateTime.UtcNow, opts.MaxAttempts);

            }

            // Save after every message, and don't let shutdown cancel this save:
            // the email is already sent, so "Done" must be recorded.
            await _db.SaveChangesAsync(CancellationToken.None);
        }
    }

    private async Task DispatchAsync(OutboxMessage message, CancellationToken ct)
    {
        switch (message.Type)
        {
            case NoteEventTypes.NoteCreatedEmail:
                {
                    var p = (JsonElement)JsonSerializer.Deserialize<object>(message.Payload)!;
                    await _emailSender.SendAsync(new EmailMessage(
                        To: _emailOptions.Value.DefaultRecipient,
                        Subject: $"Note created: {p.GetProperty("title").GetString()}",
                        Body: $"Your note \"{p.GetProperty("title").GetString()}\" was created.",
                        MessageId: message.Id
                    ));
                    break;
                }
            default:
                throw new InvalidOperationException($"Unknown outbox message type '{message.Type}'.");
        }
    }
}
