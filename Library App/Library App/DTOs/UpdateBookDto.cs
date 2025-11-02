namespace Library_App.DTOs;

public record UpdateBookDto(
    string Title, 
    int Year, 
    int AuthorId, 
    string Isbn, 
    int? PublisherId = null,
    string? Description = null,
    string? Thumbnail = null
);
