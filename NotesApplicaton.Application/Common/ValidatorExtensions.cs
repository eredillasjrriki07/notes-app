using FluentValidation;
using ValidationException = NotesApplication.Application.Common.Exceptions.ValidationException;

namespace NotesApplication.Application.Common;

public static class ValidatorExtensions
{
    public static async Task ValidateAndThrowDomainAsync<T>(this IValidator<T> validator, T instance)
    {
        var result = await validator.ValidateAsync(instance);
        if (result.IsValid) return;

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        throw new ValidationException("Validation Failed", errors);
    }
}
