using Library_App.Data;
using Library_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Library_App.Services;

public class BookService : IBookService
{
    private readonly Library_AppContext _context;
    private readonly IAuthorService _authorService;
    private readonly IPublisherService _publisherService;

    public BookService(
        Library_AppContext context, 
        IAuthorService authorService, 
        IPublisherService publisherService)
    {
        _context = context;
        _authorService = authorService;
        _publisherService = publisherService;
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .ToListAsync();
    }

    public async Task<Book?> GetAsync(int id)
    {
        return await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        return await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .FirstOrDefaultAsync(b => b.Isbn == isbn);
    }

    public async Task<Book> CreateAsync(string title, int year, int authorId, string isbn, int? publisherId)
    {
        var book = new Book
        {
            Title = title,
            Year = year,
            AuthorId = authorId,
            Isbn = isbn,
            PublisherId = publisherId
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        
        // Reload with navigation properties
        return (await GetAsync(book.Id))!;
    }

    public async Task<Book?> ImportByIsbnAsync(string isbn)
    {
        // To be implemented later with Google Books API integration
        await Task.CompletedTask;
        return null;
    }
}
