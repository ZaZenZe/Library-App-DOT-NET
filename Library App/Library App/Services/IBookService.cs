using Library_App.Models;
using Library_App.DTOs;

namespace Library_App.Services;

public interface IBookService
{
    Task<List<Book>> GetAllAsync();
    Task<Book?> GetAsync(int id);
    Task<Book?> GetByIsbnAsync(string isbn);
    Task<Book> CreateAsync(string title, int year, int authorId, string isbn, int? publisherId);
    Task<Book?> ImportByIsbnAsync(string isbn);
    Task<Book?> UpdateAsync(int id, string title, int year, int authorId, string isbn, int? publisherId);
    Task<bool> DeleteAsync(int id);
    Task<List<BookSearchResultDto>> SearchByTitleAsync(string title, int maxResults = 10);
}
