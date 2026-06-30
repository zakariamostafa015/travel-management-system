using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class BlogPost
{
    public int Id { get; set; }

    [StringLength(500)]
    public string? FeaturedImagePath { get; set; }

    [Required]
    public int CategoryId { get; set; }
    public BlogCategory? Category { get; set; }

    [Required]
    public int AuthorId { get; set; }
    public User? Author { get; set; }

    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsEvent { get; set; }

    public DateTime? PublishedDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public int ViewCount { get; set; }

    public ICollection<BlogImage> Images { get; set; } = new List<BlogImage>();
    public ICollection<BlogPostTranslation> Translations { get; set; } = new List<BlogPostTranslation>();
}
