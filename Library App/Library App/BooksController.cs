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
        try
        {
            var books = await _bookService.GetAllAsync();
            return Ok(books);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Error retrieving books",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    // GET /books/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetBook(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid book ID. ID must be greater than 0." });
            }

            var book = await _bookService.GetAsync(id);
            
            if (book == null)
            {
                return NotFound(new { error = $"Book with ID {id} not found." });
            }

            return Ok(book);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Error retrieving book",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    // GET /books/isbn/{isbn}
    [HttpGet("isbn/{isbn}")]
    public async Task<ActionResult<object>> GetBookByIsbn(string isbn)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return BadRequest(new { error = "ISBN cannot be empty." });
            }

            var book = await _bookService.GetByIsbnAsync(isbn);
            
            if (book == null)
            {
                return NotFound(new { error = $"Book with ISBN {isbn} not found." });
            }

            return Ok(book);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Error retrieving book by ISBN",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    // GET /books/search?title={title}&maxResults={maxResults}
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<object>>> SearchBooksByTitle([FromQuery] string title, [FromQuery] int maxResults = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest(new { error = "Title parameter is required and cannot be empty." });
            }

            if (maxResults < 1)
            {
                return BadRequest(new { error = "maxResults must be at least 1." });
            }

            if (maxResults > 40)
            {
                return BadRequest(new { error = "maxResults cannot exceed 40 (Google Books API limit)." });
            }

            // Returns search results from Google Books API - DOES NOT SAVE TO DATABASE
            var searchResults = await _bookService.SearchByTitleAsync(title, maxResults);
            return Ok(searchResults);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Error searching books by title",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    // POST /books
    [HttpPost]
    public async Task<ActionResult<object>> CreateBook([FromBody] CreateBookDto dto)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest(new { error = "Title cannot be empty." });
            }

            if (string.IsNullOrWhiteSpace(dto.Isbn))
            {
                return BadRequest(new { error = "ISBN cannot be empty." });
            }

            if (dto.AuthorId <= 0)
            {
                return BadRequest(new { error = "Invalid author ID." });
            }

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
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Error creating book",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    // POST /books/import/isbn/{isbn}
    [HttpPost("import/isbn/{isbn}")]
    public async Task<ActionResult<object>> ImportBookByIsbn(string isbn)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return BadRequest(new { error = "ISBN cannot be empty." });
            }

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

    // PUT /books/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateBook(int id, [FromBody] UpdateBookDto dto)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid book ID. ID must be greater than 0." });
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest(new { error = "Title cannot be empty." });
            }

            if (string.IsNullOrWhiteSpace(dto.Isbn))
            {
                return BadRequest(new { error = "ISBN cannot be empty." });
            }

            if (dto.AuthorId <= 0)
            {
                return BadRequest(new { error = "Invalid author ID." });
            }

            // Validate that the author exists
            var author = await _authorService.GetAsync(dto.AuthorId);
            if (author == null)
            {
                return BadRequest(new { error = $"Author with ID {dto.AuthorId} does not exist." });
            }

            var book = await _bookService.UpdateAsync(id, dto);
            
            if (book == null)
            {
                return NotFound(new { error = $"Book with ID {id} not found." });
            }

            return Ok(book);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Error updating book",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    // DELETE /books/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBook(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid book ID. ID must be greater than 0." });
            }

            var result = await _bookService.DeleteAsync(id);
            
            if (!result)
            {
                return NotFound(new { error = $"Book with ID {id} not found." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Error deleting book",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}