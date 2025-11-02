using Library_App.Data;
using Library_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Library_App.Services;

public class AuthorService : IAuthorService
{
    private readonly Library_AppContext _context;

    public AuthorService(Library_AppContext context)
    {
        _context = context;
    }

    public async Task<List<Author>> GetAllAsync()
    {
        return await _context.Authors
            .AsNoTracking()
            .Include(a => a.Books)
            .ThenInclude(b => b.Publisher)
            .Include(a => a.Books)
            .ThenInclude(b => b.Details)
            .ToListAsync();
    }

    public async Task<Author?> GetAsync(int id)
    {
        return await _context.Authors
            .AsNoTracking()
            .Include(a => a.Books)
            .ThenInclude(b => b.Publisher)
            .Include(a => a.Books)
            .ThenInclude(b => b.Details)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Author> CreateAsync(string name)
    {
        var author = new Author { Name = name };
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();
        return author;
    }

    public async Task<Author> GetByNameOrCreateAsync(string name)
    {
        var trimmedName = name.Trim();
        
        var existingAuthor = await _context.Authors
            .FirstOrDefaultAsync(a => a.Name == trimmedName);
        
        if (existingAuthor != null)
        {
            return existingAuthor;
        }

        return await CreateAsync(trimmedName);
    }

    public async Task<Author?> UpdateAsync(int id, string name)
    {
        var author = await _context.Authors.FindAsync(id);
        
        if (author == null)
        {
            return null;
        }

        author.Name = name.Trim();
        await _context.SaveChangesAsync();

        return await GetAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var author = await _context.Authors.FindAsync(id);
        
        if (author == null)
        {
            return false;
        }

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();
        return true;
    }
}
