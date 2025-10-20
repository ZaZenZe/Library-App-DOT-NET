using Library_App.DTOs;
using Library_App.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library_App.Controllers;

[ApiController]
[Route("books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly IAuthorService _authorService;

    public BooksController(IBookService bookService, IAuthorService authorService)
    {
        _bookService = bookService;
        _authorService = authorService;
    }

    // GET /books
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetBooks()
    {
        var books = await _bookService.GetAllAsync();
        return Ok(books);
    }

    // GET /books/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetBook(int id)
    {
        var book = await _bookService.GetAsync(id);
        
        if (book == null)
        {
            return NotFound();
        }

        return Ok(book);
    }

    // GET /books/isbn/{isbn}
    [HttpGet("isbn/{isbn}")]
    public async Task<ActionResult<object>> GetBookByIsbn(string isbn)
    {
        var book = await _bookService.GetByIsbnAsync(isbn);
        
        if (book == null)
        {
            return NotFound();
        }

        return Ok(book);
    }

    // POST /books
    [HttpPost]
    public async Task<ActionResult<object>> CreateBook([FromBody] CreateBookDto dto)
    {
        // Validate that the author exists
        var author = await _authorService.GetAsync(dto.AuthorId);
        if (author == null)
        {
            return BadRequest(new { error = $"Author with ID {dto.AuthorId} does not exist." });
        }

        var book = await _bookService.CreateAsync(
            dto.Title,
            dto.Year,
            dto.AuthorId,
            dto.Isbn,
            dto.PublisherId
        );

        return CreatedAtAction(
            nameof(GetBook),
            new { id = book.Id },
            book
        );
    }

    // POST /books/import/isbn/{isbn}
    [HttpPost("import/isbn/{isbn}")]
    public async Task<ActionResult<object>> ImportBookByIsbn(string isbn)
    {
        try
        {
            var book = await _bookService.ImportByIsbnAsync(isbn);
            
            if (book == null)
            {
                return NotFound(new { error = $"Book with ISBN {isbn} not found in external sources." });
            }

            return CreatedAtAction(
                nameof(GetBook),
                new { id = book.Id },
                book
            );
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Error importing book",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
