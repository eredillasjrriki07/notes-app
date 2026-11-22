using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotesApplication.Infrastructure.Persistence.Outbox.Options;

namespace NotesApplication.Infrastructure.Persistence.Outbox;

public sealed class OutboxWorker(
    IServiceScopeFactory _scopes,
    IOptions<OutboxOptions> _outboxOptions,
    ILogger<OutboxWorker> _logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_outboxOptions.Value.PollIntervalSeconds));

        try
        {
            do
            {
                try
                {
                    // a BackgroundService is a singleton, but DbContext is scoped: one scope per poll
                    using var scope = _scopes.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                    await processor.ProcessDueAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Outbox polling failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {

        }
    }
}
