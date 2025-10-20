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
        var authors = await _authorService.GetAllAsync();
        return Ok(authors);
    }

    // GET /authors/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetAuthor(int id)
    {
        var author = await _authorService.GetAsync(id);
        
        if (author == null)
        {
            return NotFound();
        }

        return Ok(author);
    }

    // POST /authors
    [HttpPost]
    public async Task<ActionResult<object>> CreateAuthor([FromBody] CreateAuthorDto dto)
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
}
