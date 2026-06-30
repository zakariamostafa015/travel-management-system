using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class Tour
{
    public int Id { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
    public decimal? Price { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 day")]
    public int Duration { get; set; }

    public bool IsPackage { get; set; } = true;

    [StringLength(100)]
    public string? DurationText { get; set; }

    [Required]
    public int CategoryId { get; set; }
    public TourCategory? Category { get; set; }

    [StringLength(500)]
    public string? FeaturedImagePath { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    public ICollection<TourImage> Images { get; set; } = new List<TourImage>();
    public ICollection<TourItinerary> Itineraries { get; set; } = new List<TourItinerary>();
    public ICollection<TourTranslation> Translations { get; set; } = new List<TourTranslation>();
    public ICollection<BookingRequest> BookingRequests { get; set; } = new List<BookingRequest>();
    public ICollection<TourSpot> Spots { get; set; } = new List<TourSpot>();
}
