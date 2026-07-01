using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class BlogImage
{
    public int Id { get; set; }

    [Required]
    public int BlogPostId { get; set; }
    public BlogPost BlogPost { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string ImagePath { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [StringLength(500)]
    public string? ImageLocalPath { get; set; }

    [StringLength(500)]
    public string? ThumbnailPath { get; set; }

    [StringLength(500)]
    public string? MediumPath { get; set; }

    [StringLength(200)]
    public string? AltText { get; set; }

    [StringLength(200)]
    public string? Caption { get; set; }

    public int SortOrder { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
