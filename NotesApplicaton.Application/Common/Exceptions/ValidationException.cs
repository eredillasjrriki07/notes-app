namespace NotesApplication.Application.Common.Exceptions;

public class ValidationException(string message, Dictionary<string, string[]> errors) : Exception(message)
{
    public Dictionary<string, string[]> Errors { get; } = errors;
}
