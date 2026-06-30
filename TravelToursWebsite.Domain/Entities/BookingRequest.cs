using System.ComponentModel.DataAnnotations;
using TravelToursWebsite.Domain.Enums;

namespace TravelToursWebsite.Domain.Entities;

public class BookingRequest
{
    public int Id { get; set; }

    [Required]
    public int TourId { get; set; }
    public Tour? Tour { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Number of travelers must be at least 1")]
    public int NumberOfTravelers { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime PreferredTravelDate { get; set; }

    [StringLength(200)]
    public string? SpecialRequests { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedDate { get; set; }

    [StringLength(500)]
    public string? AdminNotes { get; set; }

    public decimal? EstimatedTotal { get; set; }
}
