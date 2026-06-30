using System.ComponentModel.DataAnnotations;
using TravelToursWebsite.Domain.Enums;

namespace TravelToursWebsite.Domain.Entities;

public class SiteSettings
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [StringLength(50)]
    public string? IconClass { get; set; }

    public int SortOrder { get; set; }
    public SettingType Type { get; set; } = SettingType.Text;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
}
