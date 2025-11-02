namespace Library_App.DTOs;

public record CreateBookDto(
    string Title, 
    int Year, 
    int AuthorId, 
    string Isbn, 
    int? PublisherId = null,
    string? Description = null,
    double? AverageRating = null,
    string? SmallThumbnail = null,
    string? Thumbnail = null
);
