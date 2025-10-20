using Library_App.Data;
using Library_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Library_App.Services;

public class PublisherService : IPublisherService
{
    private readonly Library_AppContext _context;

    public PublisherService(Library_AppContext context)
    {
        _context = context;
    }

    public async Task<Publisher> GetByNameOrCreateAsync(string name)
    {
        var trimmedName = name.Trim();
        
        var existingPublisher = await _context.Publishers
            .FirstOrDefaultAsync(p => p.Name == trimmedName);
        
        if (existingPublisher != null)
        {
            return existingPublisher;
        }

        var publisher = new Publisher { Name = trimmedName };
        _context.Publishers.Add(publisher);
        await _context.SaveChangesAsync();
        return publisher;
    }
}
