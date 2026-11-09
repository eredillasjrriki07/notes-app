using Microsoft.AspNetCore.Mvc;
using NotesApplication.Api.Models;

namespace NotesApplication.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ApiOk<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.SuccessResponse(data, message));

    protected IActionResult ApiBadRequest(string message, Dictionary<string, string[]>? errors = null) =>
        BadRequest(ApiResponse<object>.ErrorResponse(message, errors));
}
