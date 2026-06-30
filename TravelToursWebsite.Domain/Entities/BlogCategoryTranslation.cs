using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class BlogCategoryTranslation
{
    public int Id { get; set; }

    [Required]
    public int BlogCategoryId { get; set; }
    public BlogCategory? BlogCategory { get; set; }

    [Required]
    [StringLength(5)]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Slug { get; set; }
}
