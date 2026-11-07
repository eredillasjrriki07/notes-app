using FluentValidation;
using NotesApplication.Application.Notes.RequestDtos;
using NotesApplication.Domain.Notes;

namespace NotesApplication.Application.Notes.Validators;

public sealed class CreateNoteRequestValidator : AbstractValidator<CreateNoteRequestDto>
{
    public CreateNoteRequestValidator()
    {
        RuleFor(x => x.Title)
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage(NoteMessages.BlankTitle)
            .Must(NoteRequestRules.TitleFits).WithMessage(NoteMessages.TitleTooLong);
        RuleFor(x => x.Content).MaximumLength(Note.ContentLength).WithMessage(NoteMessages.ContentTooLong);
    }
}

internal static class NoteRequestRules
{
    public static bool TitleFits(string? title) => (title?.Trim().Length ?? 0) <= NoteTitle.MaxLength;
}