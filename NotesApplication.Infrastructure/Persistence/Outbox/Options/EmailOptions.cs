namespace NotesApplication.Infrastructure.Persistence.Outbox.Options;

public class EmailOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public bool EnableSsl { get; set; }
    public string From { get; set; } = "notes@localhost";
    public string DefaultRecipient { get; set; } = "you@example.com";
}
