using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class TourItineraryTranslation
{
    public int Id { get; set; }

    [Required]
    public int TourItineraryId { get; set; }
    public TourItinerary? TourItinerary { get; set; }

    [Required]
    [StringLength(5)]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(200)]
    public string? Accommodation { get; set; }

    [StringLength(200)]
    public string? Meals { get; set; }
}
