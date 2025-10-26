using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Library_App.Models;

namespace Library_App.Data;

public class Library_AppContext : IdentityDbContext<IdentityUser>
{
    public Library_AppContext(DbContextOptions<Library_AppContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<BookDetails> BookDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Author to Books: one-to-many with cascade delete
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Publisher to Books: one-to-many with SetNull on delete
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Publisher)
            .WithMany(p => p.Books)
            .HasForeignKey(b => b.PublisherId)
            .OnDelete(DeleteBehavior.SetNull);

        // One-to-one Book -> BookDetails
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Details)
            .WithOne(d => d.Book!)
            .HasForeignKey<BookDetails>(d => d.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category to Books: one-to-many relationship
        // Note: This would require a many-to-many relationship or a CategoryId in Book
        // For now, keeping it simple without explicit configuration
    }
}
