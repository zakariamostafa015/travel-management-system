using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class TourImage
{
    public int Id { get; set; }

    [Required]
    public int TourId { get; set; }
    public Tour Tour { get; set; } = null!;

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
    public bool IsMainImage { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
