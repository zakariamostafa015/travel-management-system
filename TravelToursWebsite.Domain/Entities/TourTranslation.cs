using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class TourTranslation
{
    public int Id { get; set; }

    [Required]
    public int TourId { get; set; }
    public Tour? Tour { get; set; }

    [Required]
    [StringLength(5)]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string ShortDescription { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Slug { get; set; }

    [StringLength(200)]
    public string? MetaDescription { get; set; }

    [StringLength(200)]
    public string? MetaKeywords { get; set; }

    [StringLength(100)]
    public string? DurationUnit { get; set; }

    public string? ActivityHighlights { get; set; }
}
