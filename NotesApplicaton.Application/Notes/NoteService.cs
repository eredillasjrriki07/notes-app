using FluentValidation;
using Microsoft.Extensions.Logging;
using NotesApplication.Application.Common;
using NotesApplication.Application.Common.Exceptions;
using NotesApplication.Application.Events;
using NotesApplication.Application.Notes.Dtos;
using NotesApplication.Application.Notes.RequestDtos;
using NotesApplication.Domain.Common;
using NotesApplication.Domain.Notes;
using NotesApplication.Domain.Repositories;

namespace NotesApplication.Application.Notes;

public sealed class NoteService(
    ILogger<NoteService> _logger,
    INoteUnitOfWork _uow,
    INoteRepository _notes,
    INoteQueries _queries,
    IDomainEventRecorder _recorder,
    IClock _clock,
    IValidator<CreateNoteRequestDto> _createValidator,
    IValidator<UpdateNoteRequestDto> _updateValidator
) : INoteService
{
    public async Task<NoteDto> CreateAsync(CreateNoteRequestDto request, CancellationToken ct = default)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        var title = new NoteTitle(request.Title);

        var note = await _uow.ExecuteAsync(_ =>
        {
            var created = Note.Create(title, request.Content);
            _notes.Add(created);
            RecordEvent(created);
            return Task.FromResult(created);
        }, ct);

        _logger.LogInformation("Note {NoteId} created", note.Id);
        return await ReadBackAsync(note.Id, ct);
    }

    public Task<NoteDto> UpdateAsync(Guid id, UpdateNoteRequestDto request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    private async Task<Note> LoadAsync(Guid id, CancellationToken ct)
    {
        var note = await _notes.GetAsync(id, ct);
        if (note is null || note.IsArchived)
            throw new NotFoundException(NoteMessages.NotFound);
        return note;
    }

    private void RecordEvent(Note note)
    {
        foreach (var domainEvent in note.DomainEvents)
        {
            var (eventType, payload) = NoteEvents.For(domainEvent);
            _recorder.Record(eventType, NoteEventTypes.NoteEntityType, NoteEvents.EntityId(note.Id), payload);
        }
        note.ClearDomainEvents();
    }

    private async Task<NoteDto> ReadBackAsync(Guid id, CancellationToken ct)
        => await _queries.GetAsync(id, ct)
        ?? throw new InvalidOperationException($"Note {id} was saved but could not be read back.");
}
