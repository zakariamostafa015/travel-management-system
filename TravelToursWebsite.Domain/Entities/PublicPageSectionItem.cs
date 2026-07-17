using System.ComponentModel.DataAnnotations;

namespace TravelToursWebsite.Domain.Entities;

public class PublicPageSectionItem
{
    public int Id { get; set; }

    public int PublicPageSectionId { get; set; }
    public PublicPageSection? PublicPageSection { get; set; }

    [StringLength(100)]
    public string? ItemKey { get; set; }

    [StringLength(160)]
    public string? Label { get; set; }

    [StringLength(160)]
    public string? Value { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? Url { get; set; }

    [StringLength(80)]
    public string? IconClass { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
