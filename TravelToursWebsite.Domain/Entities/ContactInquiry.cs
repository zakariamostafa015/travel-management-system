using System.ComponentModel.DataAnnotations;
using TravelToursWebsite.Domain.Enums;

namespace TravelToursWebsite.Domain.Entities;

public class ContactInquiry
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public InquiryStatus Status { get; set; } = InquiryStatus.New;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedDate { get; set; }

    [StringLength(500)]
    public string? AdminNotes { get; set; }
}
