using Library_App.Models;

namespace Library_App.Services;

public interface IPublisherService
{
    Task<Publisher> GetByNameOrCreateAsync(string name);
}
