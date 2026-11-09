using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NotesApplication.Api.Models;
using NotesApplication.Application.Common.Exceptions;
using NotesApplication.Domain.Common;

namespace NotesApplication.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug("Request aborted by the client: {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            if (!httpContext.Response.HasStarted)
                httpContext.Response.StatusCode = 499;
            return true;
        }

        var (statusCode, response) = exception switch
        {
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                ApiResponse<object>.ErrorResponse(ex.Message, ex.Errors)
            ),
            NotFoundException ex => (
                StatusCodes.Status404NotFound,
                ApiResponse<object>.ErrorResponse(ex.Message)
            ),
            // Someone saved the same product after this request loaded it (the version moved). EF's own message is not
            // for users, so the sentence is fixed; clients branch on the 409, never on the text.
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                ApiResponse<object>.ErrorResponse("This record changed after you opened it. Reload and try again.")
            ),
            // An aggregate refused the change; its message is written for the user (Domain/Common).
            DomainRuleException ex => (
                StatusCodes.Status400BadRequest,
                ApiResponse<object>.ErrorResponse(ex.Message)
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse("An unexpected error occurred.")
            ),
        };

        if (statusCode >= 500)
            _logger.LogError(exception, "Server error {StatusCode}: {ExceptionType} - {Message}",
                statusCode, exception.GetType().Name, exception.Message);
        else
            _logger.LogWarning("Client error {StatusCode}: {ExceptionType} - {Message}",
                statusCode, exception.GetType().Name, exception.Message);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;

    }
}
