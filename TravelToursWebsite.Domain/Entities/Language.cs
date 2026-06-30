using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class Language
{
    public int Id { get; set; }

    [Required]
    [StringLength(5)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string CultureCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string NativeName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
}
