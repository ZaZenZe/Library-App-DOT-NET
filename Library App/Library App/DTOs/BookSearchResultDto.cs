namespace Library_App.DTOs;

public record BookSearchResultDto(
    string Title,
    List<string> Authors,
    string? Publisher,
    int? Year,
    string? Isbn,
    string? Description,
 double? AverageRating,
    string? Thumbnail
);
