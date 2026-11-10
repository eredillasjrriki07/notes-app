using Microsoft.AspNetCore.Mvc;
using NotesApplication.Api.Models;
using NotesApplication.Application.Common.Exceptions;
using NotesApplication.Application.Notes;
using NotesApplication.Application.Notes.Dtos;
using NotesApplication.Application.Notes.RequestDtos;

namespace NotesApplication.Api.Controllers.Note;

[ApiController]
[Route("api/notes")]
public class NotesController(
    INoteService _notes,
    INoteQueries _queries
) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<NoteListPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] string? q,
        [FromQuery] string? page,
        [FromQuery] string? size,
        CancellationToken ct
    )
    {
        var normalized = new NoteListQueryParams(q, page, size).Normalize();
        if (normalized.Query is null)
            return ApiBadRequest(normalized.Error!);

        return ApiOk(await _queries.ListAsync(normalized.Query, ct));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NoteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        ApiOk(await _queries.GetAsync(id, ct) ?? throw new NotFoundException(NoteMessages.NotFound));

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<NoteDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateNoteRequestDto request, CancellationToken ct)
    {
        var note = await _notes.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = note.Id }, ApiResponse<NoteDto>.SuccessResponse(note));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NoteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNoteRequestDto request, CancellationToken ct) =>
        ApiOk(await _notes.UpdateAsync(id, request, ct));

}
