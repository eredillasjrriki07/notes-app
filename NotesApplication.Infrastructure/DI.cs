using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotesApplication.Application;
using NotesApplication.Application.Common;
using NotesApplication.Application.Notes;
using NotesApplication.Application.Outbox;
using NotesApplication.Domain.Common;
using NotesApplication.Domain.Repositories;
using NotesApplication.Infrastructure.Persistence;
using NotesApplication.Infrastructure.Persistence.Events;
using NotesApplication.Infrastructure.Persistence.Outbox;
using NotesApplication.Infrastructure.Persistence.Outbox.Options;
using NotesApplication.Infrastructure.Persistence.Outbox.Repositories;
using NotesApplication.Infrastructure.Persistence.Queries;
using NotesApplication.Infrastructure.Persistence.Repositories;
using NotesApplication.Infrastructure.Time;

namespace NotesApplication.Infrastructure;

public static class NotesApplicationDependencies
{
    public static IServiceCollection AddNotesApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddNoteApplication();

        var connectionString = configuration.GetConnectionString("NotesDb") 
            ?? throw new InvalidOperationException("ConnectionStrings:NotesDb is not set.");
        services.AddDbContext<NotesDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<INoteUnitOfWork, NoteUnitOfWork>();
        services.AddScoped<IDomainEventRecorder, DomainEventRecorder>();
        services.AddScoped<IClock, SystemClock>();

        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<INoteQueries, NoteQueries>();

        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));
        services.Configure<EmailOptions>(configuration.GetSection("Email"));

        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IOutboxAdminRepository, OutboxAdminRepository>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<OutboxProcessor>();
        services.AddHostedService<OutboxWorker>();

        return services;
    }
}
