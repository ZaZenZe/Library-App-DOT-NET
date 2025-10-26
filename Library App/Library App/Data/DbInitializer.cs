using Library_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Library_App.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(Library_AppContext context)
    {
        // Ensure database is created/migrated (caller handles migrations)

        // Seed Authors if none
        if (!await context.Authors.AnyAsync())
        {
            var authors = new List<Author>
            {
                new Author { Name = "J. R. R. Tolkien" },
                new Author { Name = "George R. R. Martin" },
                new Author { Name = "Frank Herbert" },
                new Author { Name = "Robert C. Martin" }
            };

            await context.Authors.AddRangeAsync(authors);
            await context.SaveChangesAsync();
        }

        // Seed Publishers if none
        if (!await context.Publishers.AnyAsync())
        {
            var publishers = new List<Publisher>
            {
                new Publisher { Name = "Allen & Unwin" },
                new Publisher { Name = "HarperCollins" },
                new Publisher { Name = "Bantam Spectra" },
                new Publisher { Name = "Chilton Books" },
                new Publisher { Name = "Prentice Hall" }
            };

            await context.Publishers.AddRangeAsync(publishers);
            await context.SaveChangesAsync();
        }

        // Index authors and publishers for lookups
        var authorByName = await context.Authors.ToDictionaryAsync(a => a.Name);
        var publisherByName = await context.Publishers.ToDictionaryAsync(p => p.Name);

        // Seed Books if none
        if (!await context.Books.AnyAsync())
        {
            var books = new List<Book>
            {
                new Book
                {
                    Title = "The Lord of the Rings",
                    Year =1954,
                    Isbn = "9780395193956",
                    AuthorId = authorByName["J. R. R. Tolkien"].Id,
                    PublisherId = publisherByName["Allen & Unwin"].Id
                },
                new Book
                {
                    Title = "The Hobbit",
                    Year =1937,
                    Isbn = "9780547928227",
                    AuthorId = authorByName["J. R. R. Tolkien"].Id,
                    PublisherId = publisherByName["HarperCollins"].Id
                },
                new Book
                {
                    Title = "A Game of Thrones",
                    Year =1996,
                    Isbn = "9780553103540",
                    AuthorId = authorByName["George R. R. Martin"].Id,
                    PublisherId = publisherByName["Bantam Spectra"].Id
                },
                new Book
                {
                    Title = "Dune",
                    Year =1965,
                    Isbn = "9780441172719",
                    AuthorId = authorByName["Frank Herbert"].Id,
                    PublisherId = publisherByName["Chilton Books"].Id
                },
                new Book
                {
                    Title = "Clean Code",
                    Year =2008,
                    Isbn = "9780132350884",
                    AuthorId = authorByName["Robert C. Martin"].Id,
                    PublisherId = publisherByName["Prentice Hall"].Id
                }
            };

            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();
        }

        // Seed BookDetails for any book missing details
        var booksWithoutDetails = await context.Books
            .Where(b => !context.BookDetails.Any(d => d.BookId == b.Id))
            .ToListAsync();

        if (booksWithoutDetails.Count >0)
        {
            var detailsToAdd = new List<BookDetails>();
            foreach (var b in booksWithoutDetails)
            {
                var sample = GetSampleDetailsFor(b);
                detailsToAdd.Add(sample);
            }

            await context.BookDetails.AddRangeAsync(detailsToAdd);
            await context.SaveChangesAsync();
        }
    }

    private static BookDetails GetSampleDetailsFor(Book b)
    {
        // Provide themed sample data per title; fallback generic
        var title = b.Title.ToLowerInvariant();
        if (title.Contains("lord of the rings"))
        {
            return new BookDetails
            {
                BookId = b.Id,
                Description = "An epic high-fantasy saga set in Middle-earth, chronicling the quest to destroy the One Ring.",
                AverageRating =4.8,
                SmallThumbnail = "https://example.com/lotr-smallthumb.jpg",
                Thumbnail = "https://example.com/lotr-thumb.jpg",
                Small = "https://example.com/lotr-small.jpg",
                Medium = "https://example.com/lotr-medium.jpg",
                Large = "https://example.com/lotr-large.jpg"
            };
        }
        if (title.Contains("hobbit"))
        {
            return new BookDetails
            {
                BookId = b.Id,
                Description = "Bilbo Baggins embarks on an unexpected journey with dwarves to reclaim their homeland.",
                AverageRating =4.6,
                SmallThumbnail = "https://example.com/hobbit-smallthumb.jpg",
                Thumbnail = "https://example.com/hobbit-thumb.jpg",
                Small = "https://example.com/hobbit-small.jpg",
                Medium = "https://example.com/hobbit-medium.jpg",
                Large = "https://example.com/hobbit-large.jpg"
            };
        }
        if (title.Contains("game of thrones"))
        {
            return new BookDetails
            {
                BookId = b.Id,
                Description = "In the land of Westeros, noble families vie for control of the Iron Throne.",
                AverageRating =4.5,
                SmallThumbnail = "https://example.com/got-smallthumb.jpg",
                Thumbnail = "https://example.com/got-thumb.jpg",
                Small = "https://example.com/got-small.jpg",
                Medium = "https://example.com/got-medium.jpg",
                Large = "https://example.com/got-large.jpg"
            };
        }
        if (title.Contains("dune"))
        {
            return new BookDetails
            {
                BookId = b.Id,
                Description = "A science fiction epic of politics, religion, and ecology on the desert planet Arrakis.",
                AverageRating =4.7,
                SmallThumbnail = "https://example.com/dune-smallthumb.jpg",
                Thumbnail = "https://example.com/dune-thumb.jpg",
                Small = "https://example.com/dune-small.jpg",
                Medium = "https://example.com/dune-medium.jpg",
                Large = "https://example.com/dune-large.jpg"
            };
        }
        if (title.Contains("clean code"))
        {
            return new BookDetails
            {
                BookId = b.Id,
                Description = "A handbook of agile software craftsmanship with practices for writing clean, maintainable code.",
                AverageRating =4.4,
                SmallThumbnail = "https://example.com/cc-smallthumb.jpg",
                Thumbnail = "https://example.com/cc-thumb.jpg",
                Small = "https://example.com/cc-small.jpg",
                Medium = "https://example.com/cc-medium.jpg",
                Large = "https://example.com/cc-large.jpg"
            };
        }

        // Generic fallback
        return new BookDetails
        {
            BookId = b.Id,
            Description = "Sample description for seeded data.",
            AverageRating =4.0,
            SmallThumbnail = "https://example.com/sample-smallthumb.jpg",
            Thumbnail = "https://example.com/sample-thumb.jpg",
            Small = "https://example.com/sample-small.jpg",
            Medium = "https://example.com/sample-medium.jpg",
            Large = "https://example.com/sample-large.jpg"
        };
    }
}
