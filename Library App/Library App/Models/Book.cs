using System.ComponentModel.DataAnnotations;

namespace Library_App.Models;

public class Book
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Range(0, 3000)]
    public int Year { get; set; }

    [Required]
    [StringLength(20)]
    [RegularExpression(@"^(?:ISBN(?:-1[03])?:? )?(?=[0-9X]{10}$|(?=(?:[0-9]+[- ]){3})[- 0-9X]{13}$|97[89][0-9]{10}$|(?=(?:[0-9]+[- ]){4})[- 0-9]{17}$)(?:97[89][- ]?)?[0-9]{1,5}[- ]?[0-9]+[- ]?[0-9]+[- ]?[0-9X]$", 
        ErrorMessage = "Invalid ISBN format")]
    public string Isbn { get; set; } = string.Empty;

    [Required]
    public int AuthorId { get; set; }

    public int? PublisherId { get; set; }

    // Navigation properties
    public Author? Author { get; set; }
    public Publisher? Publisher { get; set; }

    // One-to-one details
    public BookDetails? Details { get; set; }
}
