using Library_App.Data;
using Library_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Library_App.Services;

public class BookService : IBookService
{
    private readonly Library_AppContext _context;
    private readonly IAuthorService _authorService;
    private readonly IPublisherService _publisherService;
    private readonly IGoogleBooksService _googleBooksService;

    public BookService(
        Library_AppContext context, 
        IAuthorService authorService, 
        IPublisherService publisherService,
        IGoogleBooksService googleBooksService)
    {
        _context = context;
        _authorService = authorService;
        _publisherService = publisherService;
        _googleBooksService = googleBooksService;
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Details)
            .ToListAsync();
    }

    public async Task<Book?> GetAsync(int id)
    {
        return await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        return await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Details)
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
        // Check if book with ISBN already exists
        var existingBook = await GetByIsbnAsync(isbn);
        if (existingBook != null)
        {
            return existingBook;
        }

        // Call Google Books API to fetch book info
        var bookInfo = await _googleBooksService.GetByIsbnAsync(isbn);
        if (bookInfo == null)
        {
            return null;
        }

        // Get or create author (use first author from the list)
        var authorName = bookInfo.Authors.FirstOrDefault() ?? "Unknown Author";
        var author = await _authorService.GetByNameOrCreateAsync(authorName);

        // Get or create publisher if available
        int? publisherId = null;
        if (!string.IsNullOrEmpty(bookInfo.Publisher))
        {
            var publisher = await _publisherService.GetByNameOrCreateAsync(bookInfo.Publisher);
            publisherId = publisher.Id;
        }

        // Create the book
        var year = bookInfo.PublishedYear ??0;
        var book = await CreateAsync(bookInfo.Title, year, author.Id, isbn, publisherId);

        // Save details
        var details = new BookDetails
        {
            BookId = book.Id,
            Description = bookInfo.Description,
            AverageRating = bookInfo.AverageRating,
            SmallThumbnail = bookInfo.SmallThumbnail,
            Thumbnail = bookInfo.Thumbnail,
            Small = bookInfo.Small,
            Medium = bookInfo.Medium,
            Large = bookInfo.Large
        };
        _context.BookDetails.Add(details);
        await _context.SaveChangesAsync();

        return await GetAsync(book.Id);
    }
}
