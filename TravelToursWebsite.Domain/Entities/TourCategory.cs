using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class TourCategory
{
    public int Id { get; set; }

    [StringLength(200)]
    public string? IconClass { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<Tour> Tours { get; set; } = new List<Tour>();
    public ICollection<TourCategoryTranslation> Translations { get; set; } = new List<TourCategoryTranslation>();
}
