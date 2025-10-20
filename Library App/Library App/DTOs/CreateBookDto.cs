namespace Library_App.DTOs;

public record CreateBookDto(string Title, int Year, int AuthorId, string Isbn, int? PublisherId = null);
