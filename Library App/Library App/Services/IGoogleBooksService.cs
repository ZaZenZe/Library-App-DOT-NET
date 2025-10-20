namespace Library_App.Services;

public interface IGoogleBooksService
{
    Task<GoogleBookInfo?> GetByIsbnAsync(string isbn);
}
