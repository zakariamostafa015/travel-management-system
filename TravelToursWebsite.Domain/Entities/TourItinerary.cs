using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class TourItinerary
{
    public int Id { get; set; }

    [Required]
    public int TourId { get; set; }
    public Tour? Tour { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Day must be at least 1")]
    public int Day { get; set; }

    public int SortOrder { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<TourItineraryTranslation> Translations { get; set; } = new List<TourItineraryTranslation>();
}
