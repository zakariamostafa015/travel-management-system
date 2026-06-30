using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class BlogPostTranslation
{
    public int Id { get; set; }

    [Required]
    public int BlogPostId { get; set; }
    public BlogPost? BlogPost { get; set; }

    [Required]
    [StringLength(5)]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Excerpt { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Slug { get; set; }

    [StringLength(200)]
    public string? MetaDescription { get; set; }

    [StringLength(200)]
    public string? MetaKeywords { get; set; }
}
