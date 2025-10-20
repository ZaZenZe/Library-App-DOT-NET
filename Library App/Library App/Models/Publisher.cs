using System.ComponentModel.DataAnnotations;

namespace Library_App.Models;

public class Publisher
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    // Navigation property - one-to-many relationship with Books
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
