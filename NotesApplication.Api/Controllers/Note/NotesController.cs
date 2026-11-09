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
        return CreatedAtAction(nameof(Get), new { id = note.Id}, ApiResponse<NoteDto>.SuccessResponse(note));
    }
}
