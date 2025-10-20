using Library_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Library_App.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(Library_AppContext context)
    {
        // Check if data already exists
        if (await context.Authors.AnyAsync())
        {
            return; // Database has been seeded
        }

        // Seed authors
        var authors = new List<Author>
        {
            new Author { Name = "J. R. R. Tolkien" },
            new Author { Name = "George R. R. Martin" },
            new Author { Name = "Frank Herbert" }
        };

        await context.Authors.AddRangeAsync(authors);
        await context.SaveChangesAsync();

        // Seed books linked to these authors
        var books = new List<Book>
        {
            new Book 
            { 
                Title = "The Lord of the Rings", 
                Year = 1954, 
                Isbn = "978-0-395-19395-6",
                AuthorId = authors[0].Id 
            },
            new Book 
            { 
                Title = "The Hobbit", 
                Year = 1937, 
                Isbn = "978-0-547-92822-7",
                AuthorId = authors[0].Id 
            },
            new Book 
            { 
                Title = "A Game of Thrones", 
                Year = 1996, 
                Isbn = "978-0-553-10354-0",
                AuthorId = authors[1].Id 
            },
            new Book 
            { 
                Title = "Dune", 
                Year = 1965, 
                Isbn = "978-0-441-17271-9",
                AuthorId = authors[2].Id 
            }
        };

        await context.Books.AddRangeAsync(books);
        await context.SaveChangesAsync();
    }
}
