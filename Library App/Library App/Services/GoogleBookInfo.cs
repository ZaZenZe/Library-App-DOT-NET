namespace Library_App.Services;

public record GoogleBookInfo(
    string Title, 
    List<string> Authors, 
    string? Publisher, 
    int? PublishedYear,
    string? Description,
    double? AverageRating,
    string? SmallThumbnail,
    string? Thumbnail,
    string? Small,
    string? Medium,
    string? Large,
    string? Isbn
);
