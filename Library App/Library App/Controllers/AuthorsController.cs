using Library_App.DTOs;
using Library_App.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library_App.Controllers;

[ApiController]
[Route("authors")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    // GET /authors
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAuthors()
    {
        try
        {
            var authors = await _authorService.GetAllAsync();
            return Ok(authors);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Error retrieving authors",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    // GET /authors/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetAuthor(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid author ID. ID must be greater than 0." });
            }

            var author = await _authorService.GetAsync(id);
            
            if (author == null)
            {
                return NotFound(new { error = $"Author with ID {id} not found." });
            }

            return Ok(author);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Error retrieving author",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    // POST /authors
    [HttpPost]
    public async Task<ActionResult<object>> CreateAuthor([FromBody] CreateAuthorDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { error = "Name is required and cannot be empty." });
            }

            var author = await _authorService.CreateAsync(dto.Name);
            
            return CreatedAtAction(
                nameof(GetAuthor),
                new { id = author.Id },
                author
            );
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Error creating author",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
