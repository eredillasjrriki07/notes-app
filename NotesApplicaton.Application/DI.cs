using NotesApplication.Application.Notes;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace NotesApplication.Application;

public static class NoteApplication
{
    public static IServiceCollection AddNoteApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(NoteApplication).Assembly);
        services.AddScoped<INoteService, NoteService>();
        return services;
    }
}
