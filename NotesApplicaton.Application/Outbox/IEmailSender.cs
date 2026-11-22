namespace NotesApplication.Application.Outbox;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
