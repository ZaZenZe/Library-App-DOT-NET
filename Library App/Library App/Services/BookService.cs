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
            .AsNoTracking()
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Details)
            .ToListAsync();
    }

    public async Task<Book?> GetAsync(int id)
    {
        return await _context.Books
            .AsNoTracking()
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        return await _context.Books
            .AsNoTracking()
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
        return (await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Details)
            .FirstAsync(b => b.Id == book.Id))!;
    }

    public async Task<Book?> ImportByIsbnAsync(string isbn)
    {
        // Check if book with ISBN already exists
        var existingBook = await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Isbn == isbn);
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

        // Return the created book including details
        return await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.Details)
            .FirstAsync(b => b.Id == book.Id);
    }

    public async Task<Book?> UpdateAsync(int id, string title, int year, int authorId, string isbn, int? publisherId)
    {
        var book = await _context.Books.FindAsync(id);
        
        if (book == null)
        {
            return null;
        }

        book.Title = title;
        book.Year = year;
        book.AuthorId = authorId;
        book.Isbn = isbn;
        book.PublisherId = publisherId;

        await _context.SaveChangesAsync();

        // Return with navigation properties
        return await GetAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        
        if (book == null)
        {
            return false;
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Book>> SearchByTitleAsync(string title)
    {
        // Search Google Books API for books with this title
        var googleBooks = await _googleBooksService.SearchByTitleAsync(title);
        
        var books = new List<Book>();
        
        foreach (var bookInfo in googleBooks)
        {
            // Check if the book already exists in our database by title
            var existingBook = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Details)
                .FirstOrDefaultAsync(b => b.Title == bookInfo.Title);
       
            if (existingBook != null)
            {
                books.Add(existingBook);
                continue;
            }

            // Create new book from Google Books data
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

            // Use ISBN or generate a placeholder
            var isbn = bookInfo.Isbn ?? $"TEMP-{Guid.NewGuid().ToString().Substring(0, 10)}";
 
            // Create the book
            var year = bookInfo.PublishedYear ?? 0;
            var book = new Book
            {
                Title = bookInfo.Title,
                Year = year,
                AuthorId = author.Id,
                Isbn = isbn,
                PublisherId = publisherId
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Save details if available
            if (bookInfo.Description != null || bookInfo.AverageRating != null || 
                bookInfo.SmallThumbnail != null || bookInfo.Thumbnail != null ||
                bookInfo.Small != null || bookInfo.Medium != null || bookInfo.Large != null)
            {
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
            }

            // Reload with navigation properties
            var createdBook = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Details)
                .FirstAsync(b => b.Id == book.Id);
      
            books.Add(createdBook);
        }

        return books;
    }
}
