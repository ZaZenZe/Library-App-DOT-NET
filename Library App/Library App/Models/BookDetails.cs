using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Library_App.Models;

namespace Library_App.Models;

public class BookDetails
{
 public int Id { get; set; }

 [Required]
 [ForeignKey(nameof(Book))]
 public int BookId { get; set; }

 [StringLength(5000)]
 public string? Description { get; set; }

 [Range(0.0,5.0)]
 public double? AverageRating { get; set; }

 [StringLength(500)]
 public string? SmallThumbnail { get; set; }

 [StringLength(500)]
 public string? Thumbnail { get; set; }

 [StringLength(500)]
 public string? Small { get; set; }

 [StringLength(500)]
 public string? Medium { get; set; }

 [StringLength(500)]
 public string? Large { get; set; }

 public Book? Book { get; set; }
}
