using System.Text.Json;

namespace Library_App.Services;

public class GoogleBooksService : IGoogleBooksService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleBooksService> _logger;

    public GoogleBooksService(HttpClient httpClient, ILogger<GoogleBooksService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<GoogleBookInfo?> GetByIsbnAsync(string isbn)
    {
        try
        {
            var url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google Books API returned status code {StatusCode} for ISBN {Isbn}", 
                    response.StatusCode, isbn);
                return null;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(jsonString);
            var root = document.RootElement;

            // Check if any items were returned
            if (!root.TryGetProperty("items", out var items) || items.GetArrayLength() == 0)
            {
                _logger.LogInformation("No books found for ISBN {Isbn}", isbn);
                return null;
            }

            // Get the first item's volume info
            var volumeInfo = items[0].GetProperty("volumeInfo");

            return ParseBookInfo(volumeInfo, isbn);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when calling Google Books API for ISBN {Isbn}", isbn);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON response from Google Books API for ISBN {Isbn}", isbn);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when calling Google Books API for ISBN {Isbn}", isbn);
            return null;
        }
    }

    public async Task<List<GoogleBookInfo>> SearchByTitleAsync(string title, int maxResults = 10)
    {
        try
        {
            // Google Books API has a max limit of 40 results per request
            // Clamp the value between 1 and 40
            maxResults = Math.Clamp(maxResults, 1, 40);
            
            // Encode the title for URL
            var encodedTitle = Uri.EscapeDataString(title);
            var url = $"https://www.googleapis.com/books/v1/volumes?q=intitle:{encodedTitle}&maxResults={maxResults}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google Books API returned status code {StatusCode} for title {Title}", 
                    response.StatusCode, title);
                return new List<GoogleBookInfo>();
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(jsonString);
            var root = document.RootElement;

            // Check if any items were returned
            if (!root.TryGetProperty("items", out var items) || items.GetArrayLength() == 0)
            {
                _logger.LogInformation("No books found for title {Title}", title);
                return new List<GoogleBookInfo>();
            }

            var books = new List<GoogleBookInfo>();
            foreach (var item in items.EnumerateArray())
            {
                var volumeInfo = item.GetProperty("volumeInfo");
                
                // Try to extract ISBN from industryIdentifiers
                string? isbn = null;
                if (volumeInfo.TryGetProperty("industryIdentifiers", out var identifiers))
                {
                    foreach (var identifier in identifiers.EnumerateArray())
                    {
                        if (identifier.TryGetProperty("type", out var type))
                        {
                            var typeStr = type.GetString();
                            if (typeStr == "ISBN_13" || typeStr == "ISBN_10")
                            {
                                isbn = identifier.TryGetProperty("identifier", out var id) ? id.GetString() : null;
                                if (isbn != null) break;
                            }
                        }
                    }
                }

                var bookInfo = ParseBookInfo(volumeInfo, isbn);
                if (bookInfo != null)
                {
                    books.Add(bookInfo);
                }
            }

            return books;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed when calling Google Books API for title {Title}", title);
            return new List<GoogleBookInfo>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON response from Google Books API for title {Title}", title);
            return new List<GoogleBookInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when calling Google Books API for title {Title}", title);
            return new List<GoogleBookInfo>();
        }
    }

    private GoogleBookInfo? ParseBookInfo(JsonElement volumeInfo, string? isbn)
    {
        try
        {
            // Extract title
            var title = volumeInfo.TryGetProperty("title", out var titleElement) 
                ? titleElement.GetString() ?? "Unknown Title"
                : "Unknown Title";

            // Extract authors
            var authors = new List<string>();
            if (volumeInfo.TryGetProperty("authors", out var authorsElement))
            {
                foreach (var author in authorsElement.EnumerateArray())
                {
                    var authorName = author.GetString();
                    if (!string.IsNullOrEmpty(authorName))
                    {
                        authors.Add(authorName);
                    }
                }
            }

            // If no authors found, add a default
            if (authors.Count == 0)
            {
                authors.Add("Unknown Author");
            }

            // Extract publisher
            string? publisher = null;
            if (volumeInfo.TryGetProperty("publisher", out var publisherElement))
            {
                publisher = publisherElement.GetString();
            }

            // Extract published year from publishedDate
            int? publishedYear = null;
            if (volumeInfo.TryGetProperty("publishedDate", out var publishedDateElement))
            {
                var dateString = publishedDateElement.GetString();
                if (!string.IsNullOrEmpty(dateString))
                {
                    // Try to parse year from date string (format can be "YYYY", "YYYY-MM", or "YYYY-MM-DD")
                    var yearString = dateString.Split('-')[0];
                    if (int.TryParse(yearString, out var year))
                    {
                        publishedYear = year;
                    }
                }
            }

            // Extract description
            string? description = null;
            if (volumeInfo.TryGetProperty("description", out var descriptionElement))
            {
                description = descriptionElement.GetString();
            }

            // Extract averageRating
            double? averageRating = null;
            if (volumeInfo.TryGetProperty("averageRating", out var averageRatingElement))
            {
                if (averageRatingElement.ValueKind == JsonValueKind.Number && averageRatingElement.TryGetDouble(out var rating))
                {
                    averageRating = rating;
                }
            }

            // Extract imageLinks
            string? smallThumbnail = null;
            string? thumbnail = null;
            string? small = null;
            string? medium = null;
            string? large = null;
            if (volumeInfo.TryGetProperty("imageLinks", out var imageLinks))
            {
                smallThumbnail = imageLinks.TryGetProperty("smallThumbnail", out var st) ? st.GetString() : null;
                thumbnail = imageLinks.TryGetProperty("thumbnail", out var t) ? t.GetString() : null;
                small = imageLinks.TryGetProperty("small", out var s) ? s.GetString() : null;
                medium = imageLinks.TryGetProperty("medium", out var m) ? m.GetString() : null;
                large = imageLinks.TryGetProperty("large", out var l) ? l.GetString() : null;
            }

            return new GoogleBookInfo(title, authors, publisher, publishedYear, description, averageRating,
                smallThumbnail, thumbnail, small, medium, large, isbn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing book info from Google Books API");
            return null;
        }
    }
}
