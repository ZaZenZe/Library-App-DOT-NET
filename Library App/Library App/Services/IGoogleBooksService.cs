namespace Library_App.Services;

public interface IGoogleBooksService
{
    Task<GoogleBookInfo?> GetByIsbnAsync(string isbn);
    Task<List<GoogleBookInfo>> SearchByTitleAsync(string title, int maxResults = 10);
}
