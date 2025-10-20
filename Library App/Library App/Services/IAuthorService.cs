using Library_App.Models;

namespace Library_App.Services;

public interface IAuthorService
{
    Task<List<Author>> GetAllAsync();
    Task<Author?> GetAsync(int id);
    Task<Author> CreateAsync(string name);
    Task<Author> GetByNameOrCreateAsync(string name);
}
