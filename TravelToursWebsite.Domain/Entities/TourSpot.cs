using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class TourSpot
{
    public int Id { get; set; }

    [Required]
    public int TourId { get; set; }
    public Tour? Tour { get; set; }

    [Required]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public decimal Latitude { get; set; }

    [Required]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public decimal Longitude { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Order must be a positive number")]
    public int Order { get; set; }

    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
}
