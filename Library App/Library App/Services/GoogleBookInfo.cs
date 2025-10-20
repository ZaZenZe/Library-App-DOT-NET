namespace Library_App.Services;

public record GoogleBookInfo(
    string Title, 
    List<string> Authors, 
    string? Publisher, 
    int? PublishedYear
);
