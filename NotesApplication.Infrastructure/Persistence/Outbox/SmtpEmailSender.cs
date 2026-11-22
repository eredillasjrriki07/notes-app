using Microsoft.Extensions.Options;
using NotesApplication.Application.Outbox;
using NotesApplication.Infrastructure.Persistence.Outbox.Options;
using System.Net.Mail;

namespace NotesApplication.Infrastructure.Persistence.Outbox;

public sealed class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var o = options.Value;

        using var client = new SmtpClient(o.Host, o.Port)
        {
            EnableSsl = o.EnableSsl,
            Timeout = 10_000
        };

        using var mail = new MailMessage(o.From, message.To, message.Subject, message.Body);
        mail.Headers.Add("Message-ID", $"<{message.MessageId}@notesapplication>");

        await client.SendMailAsync(mail, ct);
    }
}
